using System;
using System.Collections.Generic;

using BNetzAWebServiceClient.WsVersUnterbrechungenStrom;

using log4net;

using FileHelpers;

namespace BNetzAWebServiceClient
{
    class TransferStrom
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TransferStrom));

        private EinmaldatenStrom einmaldaten;
        private String transaktionsnummer;
        private WsVersUnterbrechungenStromSoapClient service = new WsVersUnterbrechungenStromSoapClient();
        private List<Unterbrechung> unterbrechungen = new List<Unterbrechung>();


        public void EinmaldatenEinlesen()
        {
            FileHelperEngine engine = new FileHelperEngine(typeof(EinmaldatenStrom));
            engine.Options.IgnoreFirstLines = 1;            
            EinmaldatenStrom[] res = engine.ReadFile("einmaldaten-strom.csv") as EinmaldatenStrom[];
            einmaldaten = res[0];
        }

        public void UnterbrechungenEinlesen()
        {
            FileHelperEngine engine = new FileHelperEngine(typeof(UnterbrechungStrom));
            engine.Options.IgnoreFirstLines = 1;
            UnterbrechungStrom[] res = engine.ReadFile("unterbrechungen-strom.csv") as UnterbrechungStrom[];
            foreach (UnterbrechungStrom unterbrechung in res)
            {
                Unterbrechung u = new Unterbrechung();
                u.LfdNr = unterbrechung.LfdNr;

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
                ud.VSU_1_13 = unterbrechung.VSU_1_13;

                UnterbrechungDetail[] details = { ud };

                u.Details = details;
                unterbrechungen.Add(u);
            }
        }

        public string TransaktionBeginnen()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };

            BeginnTranAntwort tranAntwort = service.BeginnTransaktion(einmaldaten.Betriebsnummer, einmaldaten.Kontrollnummer, einmaldaten.Netznummer,
                einmaldaten.Berichtsjahr, einmaldaten.Leermeldung, 1, unterbrechungen.Count, unterbrechungen.Count, TransBeginnArtEnum.EchtDaten);
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
