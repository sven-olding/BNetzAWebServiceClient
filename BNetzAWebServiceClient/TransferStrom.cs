
using BNetzAWebServiceClient.WsVersUnterbrechungenStrom;
using Domino;

using log4net;

using System;
using System.Collections.Generic;

namespace BNetzAWebServiceClient
{
    class TransferStrom
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(typeof(TransferStrom));

        private EinmaldatenStrom einmaldaten;
        private String transaktionsnummer;
        private WsVersUnterbrechungenStromSoapClient service = new WsVersUnterbrechungenStromSoapClient();
        private List<Unterbrechung> unterbrechungen = new List<Unterbrechung>();
        private Domino.NotesDatabase db;
        private bool test;

        public TransferStrom(Domino.NotesDatabase db, bool test)
        {
            this.db = db;
            this.test = test;
        }

        public void EinmaldatenEinlesen()
        {
            NotesView viw = db.GetView("VABNAEinmaldaten2022");
            NotesDocument document = viw.GetFirstDocument();
            einmaldaten = new EinmaldatenStrom();
            einmaldaten.Berichtsjahr = Convert.ToInt32(document.GetItemValue("berichtsJahrBT")[0]);
            einmaldaten.Betriebsnummer = Convert.ToInt32(document.GetItemValue("Betriebsnummer")[0]);
            einmaldaten.Kontrollnummer = document.GetItemValue("Kontrollnummer")[0];
            einmaldaten.Netznummer = Convert.ToInt32(document.GetItemValue("NetznummerBT")[0]);
            einmaldaten.Leermeldung = Convert.ToBoolean(document.GetItemValue("leermeldungBT")[0]);
            einmaldaten.VSU_1_11_1 = document.GetItemValue("VSU_1_11_1")[0];
            einmaldaten.VSU_1_11_2 = Convert.ToDecimal(document.GetItemValue("VSU_1_11_2")[0]);
            einmaldaten.VSU_1_2 = Convert.ToInt32(document.GetItemValue("VSU_1_2")[0]);
            einmaldaten.VSU_1_3 = Convert.ToDecimal(document.GetItemValue("VSU_1_3")[0]);
            einmaldaten.VSU_1_4 = Convert.ToDecimal(document.GetItemValue("VSU_1_4")[0]);
            einmaldaten.VSU_1_5 = Convert.ToDecimal(document.GetItemValue("VSU_1_5")[0]);
            einmaldaten.VSU_1_6 = Convert.ToDecimal(document.GetItemValue("VSU_1_6")[0]);
            einmaldaten.VSU_1_7 = Convert.ToDecimal(document.GetItemValue("VSU_1_7")[0]);
            einmaldaten.VSU_1_8 = Convert.ToDecimal(document.GetItemValue("VSU_1_8")[0]);
            einmaldaten.VSU_1_9 = document.GetItemValue("VSU_1_9")[0];
        }

        private UnterbrechungStrom GetUnterbrechungFromDoc(NotesDocument doc)
        {
            UnterbrechungStrom u = new UnterbrechungStrom();
            
            u.VSU_1_14 = false;
            u.VSU_1_15 = false;
            bool is20kv = doc.GetItemValue("fdClassification")[0].ToString().Equals("20 kV");
            bool is1kv = doc.GetItemValue("fdClassification")[0].ToString().Equals("1 kV");
            u.VSU_1_16 = is20kv;
            u.VSU_1_17 = is1kv;
            var start = doc.GetItemValue("fdErrorStart")[0];
            u.VSU_1_18 = start;
            var ende = doc.GetItemValue("fdErrorEnde")[0];
            TimeSpan ts = ende - start;
            u.VSU_1_19 = Convert.ToInt32(ts.TotalMinutes);
            var templateName = doc.GetItemValue("TemplateName")[0].ToString();
            if (templateName.Equals("Geplante VU"))
            {
                u.VSU_1_20 = "G";
            }
            else if (templateName.Equals("Geplante VU Strom"))
            {
                u.VSU_1_20 = "G";
            }
            else
            {
                u.VSU_1_20 = "N";
            }

            var anlass = doc.GetItemValue("fdStörungsanlass")[0].ToString();

            u.StoerungsAnlass = "XX";
            switch (anlass)
            {
                case "Atmosphärische Einwirkung":
                    u.StoerungsAnlass = "AE";
                    break;
                case "Einwirkung Dritter":
                    u.StoerungsAnlass = "ED";
                    break;
                case "Zuständigkeit des Netzbetreibers / Kein erkennbarer Anlass":
                    u.StoerungsAnlass = "ZBN";
                    break;
                case "Rückwirkungsstörung":
                    u.StoerungsAnlass = "RWS";
                    break;
                case "Höhere Gewalt":
                    u.StoerungsAnlass = "HG";
                    break;
                case "Sonstiges":
                    u.StoerungsAnlass = "S";
                    break;
                case "Zählerwechsel":
                    u.StoerungsAnlass = "ZW";
                    break;
            }
            u.VSU_1_36 = " ";
            if (is1kv)
            {
                u.KTrafo = Convert.ToDecimal(doc.GetItemValue("fdLetztverbraucher")[0]);
            }
            else if (is20kv)
            {
                u.KTrafo = Convert.ToDecimal(doc.GetItemValue("fdMVAKundentrafo")[0]);
            }
            else
            {
                u.KTrafo = 0;
            }
            if (is1kv)
            {
                u.KTrafo_Produkt = Convert.ToDecimal(doc.GetItemValue("fdKundenMin")[0]);
            }
            else if (is20kv)
            {
                u.KTrafo_Produkt = Convert.ToDecimal(doc.GetItemValue("fdMVAKundentrafoMin")[0]);
            }
            else
            {
                u.KTrafo_Produkt = 0;
            }
            if (is20kv)
            {
                u.NTrafo = Convert.ToDecimal(doc.GetItemValue("fdNetztrafo")[0]);
                u.NTrafo_Produkt = Convert.ToDecimal(doc.GetItemValue("fdMVANetztrafoMin")[0]);
            }
            else
            {
                u.NTrafo = 0;
                u.NTrafo_Produkt = 0;
            }
            if (is1kv)
            {
                u.NetzebeneID = "NS";
            }
            else if (is20kv)
            {
                u.NetzebeneID = "MS";
            }
            else
            {
                u.NetzebeneID = "XX";
            }
            return u;
        }

        public void UnterbrechungenEinlesen()
        {
            NotesView viw = db.GetView("03. Statistik\\BNADatenStrom2022B");
            NotesDocument doc = viw.GetFirstDocument();
            int lfdNr = 0;
            while (doc != null)
            {
                lfdNr++;
                UnterbrechungStrom unterbrechung = GetUnterbrechungFromDoc(doc);
                Unterbrechung u = new Unterbrechung();                
                u.LfdNr = lfdNr;
                

                u.VSU_1_14 = unterbrechung.VSU_1_14;
                u.VSU_1_15 = unterbrechung.VSU_1_15;
                u.VSU_1_16 = unterbrechung.VSU_1_16;
                u.VSU_1_17 = unterbrechung.VSU_1_17;
                u.VSU_1_18 = unterbrechung.VSU_1_18;
                u.VSU_1_19 = unterbrechung.VSU_1_19;
                u.VSU_1_36 = unterbrechung.VSU_1_36;

                String anlass = unterbrechung.StoerungsAnlass;
                switch (anlass)
                {
                    case "AE":
                        u.StoerungsAnlass = StoerungsAnlassEnum.AE;
                        break;
                    case "ED":
                        u.StoerungsAnlass = StoerungsAnlassEnum.ED;
                        break;
                    case "ZBN":
                        u.StoerungsAnlass = StoerungsAnlassEnum.ZBN;
                        break;
                    case "RWS":
                        u.StoerungsAnlass = StoerungsAnlassEnum.RWS;
                        break;
                    case "HG":
                        u.StoerungsAnlass = StoerungsAnlassEnum.HG;
                        break;
                    case "ZW":
                        u.StoerungsAnlass = StoerungsAnlassEnum.ZW;
                        break;
                    case "S":
                        u.StoerungsAnlass = StoerungsAnlassEnum.S;
                        break;
                    default:
                        u.StoerungsAnlass = StoerungsAnlassEnum.NichtGesetzt;
                        break;
                }

                switch (unterbrechung.VSU_1_20)
                {
                    case "N":
                        u.Unterbrechungsart = UnterbrechungsArtEnum.U;
                        break;
                    case "G":
                        u.Unterbrechungsart = UnterbrechungsArtEnum.G;
                        break;
                    default:
                        u.Unterbrechungsart = UnterbrechungsArtEnum.NichtGesetzt;
                        break;
                }

                UnterbrechungDetail ud = new UnterbrechungDetail();
                ud.KTrafo = unterbrechung.KTrafo;
                ud.KTrafo_Produkt = unterbrechung.KTrafo_Produkt;
                switch (unterbrechung.NetzebeneID)
                {
                    case "NS":
                        ud.NetzebeneID = NetzebeneEnum.NS;
                        break;
                    case "MS":
                        ud.NetzebeneID = NetzebeneEnum.MS;
                        break;
                    case "HS":
                        ud.NetzebeneID = NetzebeneEnum.HS;
                        break;
                    case "HoeS":
                        ud.NetzebeneID = NetzebeneEnum.HoeS;
                        break;
                    default:
                        ud.NetzebeneID = NetzebeneEnum.NichtGesetzt;
                        break;
                }
                ud.NTrafo = unterbrechung.NTrafo;
                ud.NTrafo_Produkt = unterbrechung.NTrafo_Produkt;
                ud.VSU_1_13 = lfdNr;

                UnterbrechungDetail[] details = { ud };

                u.Details = details;
                unterbrechungen.Add(u);

                doc = viw.GetNextDocument(doc);
            }
        }

        public string TransaktionBeginnen()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };

            TransBeginnArtEnum transBeginnArtEnum = test ? TransBeginnArtEnum.TestAkzeptiert : TransBeginnArtEnum.EchtDaten;
            BeginnTranAntwort tranAntwort = service.BeginnTransaktion(einmaldaten.Betriebsnummer, einmaldaten.Kontrollnummer, einmaldaten.Netznummer,
                einmaldaten.Berichtsjahr, einmaldaten.Leermeldung, 1, unterbrechungen.Count, unterbrechungen.Count, transBeginnArtEnum);
            transaktionsnummer = tranAntwort.Transaktionsnummer;

            log.Info(tranAntwort.Meldung);
            log.Info("Transaktionsnummer: " + transaktionsnummer);

            return transaktionsnummer;
        }

        public void TransaktionAbschliessen()
        {
            TranAntwort tranAntwort = service.AbschlussTransaktion(transaktionsnummer);
            log.Debug(tranAntwort.MeldungsCode + ": " + tranAntwort.Meldung);
        }

        public bool EinmaldatenSenden()
        {
            Einmalig e = new Einmalig();
            e.VSU_1_11_1 = einmaldaten.VSU_1_11_1;
            e.VSU_1_11_2 = einmaldaten.VSU_1_11_2;
            e.VSU_1_2 = einmaldaten.VSU_1_2;
            e.VSU_1_3 = einmaldaten.VSU_1_3;
            e.VSU_1_4 = einmaldaten.VSU_1_4;
            e.VSU_1_5 = einmaldaten.VSU_1_5;
            e.VSU_1_6 = einmaldaten.VSU_1_6;
            e.VSU_1_7 = einmaldaten.VSU_1_7;
            e.VSU_1_8 = einmaldaten.VSU_1_8;
            e.VSU_1_9 = einmaldaten.VSU_1_9;

            TranAntwort tranAntwort = service.TransEinmalig(transaktionsnummer, e);
            log.Info("Antwort: " + tranAntwort.MeldungsCode + ": " + tranAntwort.Meldung);
            log.Info("StatusWs: " + tranAntwort.StatusWs.ToString());
            return tranAntwort.StatusWs.Equals(TransStatusEnum.Akzeptiert);
        }

        public bool UnterbrechungenSenden()
        {
            TranUnterbrAntwort tranAntwort = service.TransUnterbrechungen(transaktionsnummer, 1, unterbrechungen.ToArray());
            log.Info("Antwort: " + tranAntwort.MeldungsCode + ": " + tranAntwort.Meldung);
            log.Info("StatusWs: " + tranAntwort.StatusWs.ToString());
            log.Info("AnzahlEmpfangen: " + tranAntwort.AnzahlEmpfangen);

            return tranAntwort.StatusWs.Equals(TransStatusEnum.Akzeptiert);
        }

        public string AntwortVorpruefungHolen(string transnr)
        {
            if (transnr != null)
            {
                transaktionsnummer = transnr;
            }

            VorPruefAntwort tranAntwort = service.ErgebnisVorPruefung(einmaldaten.Betriebsnummer, einmaldaten.Kontrollnummer, transaktionsnummer);
            log.Info("StatusWs = " + tranAntwort.StatusWs.ToString());
            log.Info("Meldung = " + tranAntwort.Meldung);

            if (tranAntwort.Vorpruefliste != null)
            {

                DatenVorpruefEintrag[] liste = tranAntwort.Vorpruefliste;

                foreach (DatenVorpruefEintrag e in liste)
                {
                    log.Error("LfdNr = " + e.LfdNr);
                    log.Error("Fehlergruppe = " + e.FehlerGruppe.ToString());
                    log.Error("Fehlernummer = " + e.FehlerNummer);
                    log.Error("Beschreibung = " + e.Beschreibung);
                    log.Error("Feldbezeichnung = " + e.FeldBezeichnung);
                    log.Info("------------------------------------------");
                }
            }

            return tranAntwort.Meldung;
        }
    }
}
