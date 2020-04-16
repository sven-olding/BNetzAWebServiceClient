using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BNetzAWebServiceClient.WS_VersUnterbrechungGas;

using log4net;
using FileHelpers;

namespace BNetzAWebServiceClient
{
    class TransferGas
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TransferGas));

        private const string KONTROLLNUMMER = "INKAVZYC";

        private EinmaldatenGas einmaldaten;
        private string transaktionsnummer;

        private WS_VersUnterbrechungGasSoapClient service = new WS_VersUnterbrechungGasSoapClient();
        private List<Unterbrechung> unterbrechungen = new List<Unterbrechung>();


        public void EinmaldatenEinlesen()
        {
            FileHelperEngine engine = new FileHelperEngine(typeof(EinmaldatenGas));
            engine.Options.IgnoreFirstLines = 1;
            EinmaldatenGas[] res = engine.ReadFile("einmaldaten-gas.csv") as EinmaldatenGas[];
            einmaldaten = res[0];
        }

        public void UnterbrechungenEinlesen()
        {
            FileHelperEngine engine = new FileHelperEngine(typeof(UnterbrechungGas));
            engine.Options.IgnoreFirstLines = 1;
            UnterbrechungGas[] res = engine.ReadFile("unterbrechungen-gas.csv") as UnterbrechungGas[];
            foreach (UnterbrechungGas unterbrechung in res)
            {
                Unterbrechung u = new Unterbrechung();
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
                        u.UnterbrechungsArt = UnterbrechungsArtEnum.Ungeplant;
                        break;
                    case 2:
                        u.UnterbrechungsArt = UnterbrechungsArtEnum.Geplant;
                        break;
                    default:
                        u.UnterbrechungsArt = UnterbrechungsArtEnum.NichtGesetzt;
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
            }
        }

        public string TransaktionBeginnen()
        {
            BeginnTransAntwort tranAntwort = service.BeginnTransaktion(einmaldaten.Bnr, KONTROLLNUMMER, einmaldaten.Nnr, einmaldaten.Berichtsjahr,
                einmaldaten.Leermeldung, 1, unterbrechungen.Count, unterbrechungen.Count, TransBeginnArtEnum.EchtDaten);

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

            return tranAntwort.StatusWS.Equals(TransStatusEnum.Akzeptiert);
        }

        public bool UnterbrechungenSenden()
        {
            TransUnterbrechungenAntwort tranAntwort = service.TransUnterbrechungen(transaktionsnummer, 1, unterbrechungen.ToArray());
            log.Info("Antwort: " + tranAntwort.Meldungscode + ": " + tranAntwort.Meldung);
            log.Info("StatusWs: " + tranAntwort.StatusWS.ToString());
            return tranAntwort.StatusWS.Equals(TransStatusEnum.Akzeptiert);
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
