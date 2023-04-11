
using BNetzAWebServiceClient.WS_VersUnterbrechungGas;
using Domino;
using log4net;

using System;

using System.Collections.Generic;

namespace BNetzAWebServiceClient
{
    class TransferGas
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(typeof(TransferGas));

        private const string KONTROLLNUMMER = "INKAVZYC";

        private EinmaldatenGas einmaldaten;
        private string transaktionsnummer;

        private WS_VersUnterbrechungGasSoapClient service = new WS_VersUnterbrechungGasSoapClient();
        private List<WS_VersUnterbrechungGas.Unterbrechung> unterbrechungen = new List<WS_VersUnterbrechungGas.Unterbrechung>();
        private Domino.NotesDatabase db;
        private bool test;

        public TransferGas(Domino.NotesDatabase db, bool test)
        {
            this.db = db;
            this.test = test;
        }

        public void EinmaldatenEinlesen()
        {
            NotesView viw = db.GetView("VABNAEinmaldatenGas2022");
            NotesDocument document = viw.GetFirstDocument();
            einmaldaten = new EinmaldatenGas();
            einmaldaten.A_qu_kg1 = Convert.ToDecimal(document.GetItemValue("A_qu_kg1")[0]);
            einmaldaten.A_qu_kg2 = Convert.ToDecimal(document.GetItemValue("A_qu_kg2")[0]);
            einmaldaten.AnzahlVUGesamt = Convert.ToDecimal(document.GetItemValue("anzahlVUGesamt")[0]);
            einmaldaten.Berichtsjahr = Convert.ToInt32(document.GetItemValue("berichtsjahr")[0]);
            einmaldaten.Bnr = Convert.ToInt32(document.GetItemValue("bnr")[0]);
            einmaldaten.Leermeldung = Convert.ToBoolean(document.GetItemValue("leermeldung")[0]);
            einmaldaten.Nnr = Convert.ToInt32(document.GetItemValue("nnr")[0]);
            einmaldaten.VUG_Ansprech = document.GetItemValue("vug_Ansprech")[0];
            einmaldaten.VUG_ANZ_LV = Convert.ToInt32(document.GetItemValue("VUG_ANZ_LV")[0]);
            einmaldaten.VUG_Bericht_Text = document.GetItemValue("vug_Bericht_Text")[0];
            einmaldaten.VUG_Dopplung = Convert.ToInt32(document.GetItemValue("vug_dopplung")[0]);
            einmaldaten.VUG_Kommentar_Text = document.GetItemValue("VUG_Kommentar_Text")[0];
            einmaldaten.VUG_Kontakt = document.GetItemValue("VUG_Kontakt")[0];
            einmaldaten.VUG_NB_Art = Convert.ToInt32(document.GetItemValue("VUG_NB_Art")[0]);
            einmaldaten.VUG_SUM_KAP_NACH = Convert.ToInt32(document.GetItemValue("VUG_SUM_KAP_NACH")[0]);
            einmaldaten.VUG_SUM_KAP_VOR = Convert.ToInt32(document.GetItemValue("VUG_SUM_KAP_VOR")[0]);
            einmaldaten.VUG_SUM_LST = Convert.ToInt32(document.GetItemValue("VUG_SUM_LST")[0]);
        }

        private UnterbrechungGas GetUnterbrechungFromDoc(NotesDocument doc)
        {
            UnterbrechungGas u = new UnterbrechungGas();

            u.stAnlassID = Convert.ToInt32(doc.GetItemValue("fdUnterbrechungsanlassGAS")[0]);
            u.UnterbrechungsArt = 1;
            u.vug_ANM = doc.GetItemValue("fdAnmerkungGAS")[0];
            u.vug_AUSM_EG1 = Convert.ToInt32(doc.GetItemValue("fdAnzUnterbrLetzV")[0]);
            u.vug_AUSM_EG2 = Convert.ToInt32(doc.GetItemValue("fdAusmassGAS")[0]);

            DateTime dt = doc.GetItemValue("fdErrorStart")[0];
            dt.AddHours(-1);
            u.vug_Beginndatum = dt;
            u.vug_Beginnzeit = dt;

            var start = doc.GetItemValue("fdErrorStart")[0];
            var ende = doc.GetItemValue("fdErrorEnde")[0];
            TimeSpan ts = ende - start;
            u.vug_Dauer = Convert.ToInt32(ts.TotalMinutes);

            u.vug_EG = Convert.ToInt32(doc.GetItemValue("fdErfassungsgruppeGAS")[0]);
            u.vug_ETSO = "";
            if (doc.GetItemValue("fdErfassungsgruppeGAS")[0].ToString().Equals("1"))
            {
                u.vug_KMIN_EG1 = Convert.ToInt32(doc.GetItemValue("fdKundenminutenGas")[0]);
            }
            else
            {
                u.vug_KMIN_EG1 = 0;
            }
            if (doc.GetItemValue("fdErfassungsgruppeGAS")[0].ToString().Equals("2"))
            {
                u.vug_KMIN_EG2 = Convert.ToInt32(doc.GetItemValue("fdKundenminutenGAS")[0]);
            }
            else
            {
                u.vug_KMIN_EG2 = 0;
            }
            u.vug_nb_name = "GEW Wilhelmshaven GmbH";
            u.vug_NB_betroffen = "";

            return u;
        }


        public void UnterbrechungenEinlesen()
        {
            NotesView viw = db.GetView("Statistik\\BNADatenGas2022exp");
            NotesDocument doc = viw.GetFirstDocument();
            int lfdNr = 0;
            while (doc != null)
            {
                lfdNr++;

                UnterbrechungGas unterbrechung = GetUnterbrechungFromDoc(doc);
                unterbrechung.LfdNr = lfdNr;

                WS_VersUnterbrechungGas.Unterbrechung u = new WS_VersUnterbrechungGas.Unterbrechung();
                u.LfdNr = unterbrechung.LfdNr;

                switch (unterbrechung.stAnlassID)
                {
                    case 1:
                        u.stAnlassID = UnterbrechungsAnlassEnum.VD;
                        break;
                    case 2:
                        u.stAnlassID = UnterbrechungsAnlassEnum.VN;
                        break;
                    case 3:
                        u.stAnlassID = UnterbrechungsAnlassEnum.FS;
                        break;
                    case 4:
                        u.stAnlassID = UnterbrechungsAnlassEnum.HG;
                        break;
                    case 5:
                        u.stAnlassID = UnterbrechungsAnlassEnum.US;
                        break;
                    case 6:
                        u.stAnlassID = UnterbrechungsAnlassEnum.ZW;
                        break;
                    case 7:
                        u.stAnlassID = UnterbrechungsAnlassEnum.IM;
                        break;
                    case 8:
                        u.stAnlassID = UnterbrechungsAnlassEnum.IA;
                        break;
                    case 9:
                        u.stAnlassID = UnterbrechungsAnlassEnum.GS;
                        break;
                    default:
                        u.stAnlassID = UnterbrechungsAnlassEnum.NichtGesetzt;
                        break;
                }

                switch (unterbrechung.UnterbrechungsArt)
                {
                    case 1:
                        u.UnterbrechungsArt = WS_VersUnterbrechungGas.UnterbrechungsArtEnum.Ungeplant;
                        break;
                    case 2:
                        u.UnterbrechungsArt = WS_VersUnterbrechungGas.UnterbrechungsArtEnum.Geplant;
                        break;
                    default:
                        u.UnterbrechungsArt = WS_VersUnterbrechungGas.UnterbrechungsArtEnum.NichtGesetzt;
                        break;
                }

                u.vug_ANM = unterbrechung.vug_ANM;
                u.vug_AUSM_EG1 = unterbrechung.vug_AUSM_EG1;
                u.vug_AUSM_EG2 = unterbrechung.vug_AUSM_EG2;
                u.vug_Beginndatum = unterbrechung.vug_Beginndatum;
                u.vug_BeginnZeit = unterbrechung.vug_Beginnzeit;
                u.vug_Dauer = unterbrechung.vug_Dauer;
                u.vug_EG = unterbrechung.vug_EG;
                u.vug_ETSO = unterbrechung.vug_ETSO;
                u.vug_KMIN_EG1 = unterbrechung.vug_KMIN_EG1;
                u.vug_KMIN_EG2 = unterbrechung.vug_KMIN_EG2;
                switch (unterbrechung.vug_NB_betroffen)
                {
                    case "1":
                        u.vug_NB_betroffen = Betroffen.Letztverbraucher;
                        break;
                    case "2":
                        u.vug_NB_betroffen = Betroffen.nachgelagerterNetzbetreiber;
                        break;
                    default:
                        u.vug_NB_betroffen = Betroffen.NichtGesetzt;
                        break;
                }
                u.vug_nb_name = unterbrechung.vug_nb_name;

                unterbrechungen.Add(u);
                doc = viw.GetNextDocument(doc);
            }
        }

        public string TransaktionBeginnen()
        {
            WS_VersUnterbrechungGas.TransBeginnArtEnum transBeginnArtEnum = test ? WS_VersUnterbrechungGas.TransBeginnArtEnum.TestAkzeptiert : WS_VersUnterbrechungGas.TransBeginnArtEnum.EchtDaten;
            BeginnTransAntwort tranAntwort = service.BeginnTransaktion(einmaldaten.Bnr, KONTROLLNUMMER, einmaldaten.Nnr, einmaldaten.Berichtsjahr,
                einmaldaten.Leermeldung, 1, unterbrechungen.Count, unterbrechungen.Count, transBeginnArtEnum);

            transaktionsnummer = tranAntwort.TransNr;

            log.Info(tranAntwort.Meldung);
            log.Info("Transaktionsnummer: " + transaktionsnummer);

            return transaktionsnummer;
        }

        public void TransaktionAbschliessen()
        {
            Antwort tranAntwort = service.AbschlussTransaktion(transaktionsnummer);
            log.Info(tranAntwort.Meldung);
        }

        public bool EinmaldatenSenden()
        {
            Netzdaten n = new Netzdaten();
            n.A_qu_kg1 = einmaldaten.A_qu_kg1;
            n.A_qu_kg2 = einmaldaten.A_qu_kg2;
            n.vug_ANSPRECH = einmaldaten.VUG_Ansprech;
            n.vug_ANZ_LV = einmaldaten.VUG_ANZ_LV;
            n.vug_Bericht_Text = einmaldaten.VUG_Bericht_Text;

            switch (einmaldaten.VUG_Dopplung)
            {
                case 1:
                    n.vug_dopplung = Dopplung.KeineDopplung;
                    break;
                case 2:
                    n.vug_dopplung = Dopplung.DopplungVorhanden;
                    break;
                default:
                    n.vug_dopplung = Dopplung.NichtGesetzt;
                    break;
            }

            n.vug_kommentar_text = einmaldaten.VUG_Kommentar_Text;
            n.vug_Kontakt = einmaldaten.VUG_Kontakt;
            n.Vug_NB_Art = einmaldaten.VUG_NB_Art;
            n.vug_SUM_KAP_NACH = einmaldaten.VUG_SUM_KAP_NACH;
            n.vug_SUM_KAP_VOR = einmaldaten.VUG_SUM_KAP_VOR;
            n.vug_SUM_LST = einmaldaten.VUG_SUM_LST;

            Antwort tranAntwort = service.TransNetzdaten(transaktionsnummer, n);
            log.Info("Antwort: " + tranAntwort.Meldungscode + ": " + tranAntwort.Meldung);
            log.Info("StatusWs: " + tranAntwort.StatusWS.ToString());

            return tranAntwort.StatusWS.Equals(WS_VersUnterbrechungGas.TransStatusEnum.Akzeptiert);
        }

        public bool UnterbrechungenSenden()
        {
            TransUnterbrechungenAntwort tranAntwort = service.TransUnterbrechungen(transaktionsnummer, 1, unterbrechungen.ToArray());
            log.Info("Antwort: " + tranAntwort.Meldungscode + ": " + tranAntwort.Meldung);
            log.Info("StatusWs: " + tranAntwort.StatusWS.ToString());
            return tranAntwort.StatusWS.Equals(WS_VersUnterbrechungGas.TransStatusEnum.Akzeptiert);
        }

        public string AntwortVorpruefungHolen(string transnr)
        {
            if (transnr != null)
            {
                transaktionsnummer = transnr;
            }

            PruefAntwort tranAntwort = service.ErgebnisPruefung(transaktionsnummer);

            log.Info("StatusWs = " + tranAntwort.StatusWS.ToString());


            Fehler[] fehlerliste = tranAntwort.Fehlerliste;
            foreach (Fehler fehler in fehlerliste)
            {
                log.Error("LfdNr = " + fehler.Laufendenummer);
                log.Error("Fehlerart = " + fehler.FehlerArt.ToString());
                log.Error("Fehlernummer = " + fehler.FehlerNummer);
                log.Error("Beschreibung = " + fehler.Beschreibung);
                log.Error("Feldbezeichnung = " + fehler.Bezeichnung);
                log.Info("------------------------------------------");
            }

            return tranAntwort.Meldung;
        }
    }
}
