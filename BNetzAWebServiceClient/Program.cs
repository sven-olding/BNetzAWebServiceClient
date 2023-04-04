
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using CommandLine;

namespace BNetzAWebServiceClient
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private const string FILE_NAME_TRANSNR_STROM = "transaktionsnummer-strom.txt";

        private const string FILE_NAME_TRANSNR_GAS = "transaktionsnummer-gas.txt";

        public class Options
        {
            [Option('a', "antwort", HelpText = "Holt die Antwort der Vorprüfung")]
            public bool AntwortHolen { get; set; }
            [Option('t', "type", Required = true, HelpText = "Art des Serviceaufruf (Gas oder Strom = g oder s)")]
            public string Type { get; set; }
        }

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(Certificates.ValidateRemoteCertificate);

            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                log.Info("START");

                switch (o.Type.ToUpper())
                {
                    case "S":
                        if (o.AntwortHolen)
                        {
                            StromAntwortHolen();
                        }
                        else
                        {
                            StromDatenSenden();
                        }
                        break;

                    case "G":
                        if (o.AntwortHolen)
                        {
                            GasAntwortHolen();
                        }
                        else
                        {
                            GasDatenSenden();
                        }
                        break;

                    default:
                        break;
                }

                log.Info("ENDE");
            });


        }

        /// <summary>
        /// Versand der Einmaldaten und Versorgungsunterbrechungen Strom
        /// </summary>
        private static void StromDatenSenden()
        {
            try
            {
                TransferStrom ts = new TransferStrom();
                ts.EinmaldatenEinlesen();
                ts.UnterbrechungenEinlesen();

                String transaktionsnummer = ts.TransaktionBeginnen();
                if (transaktionsnummer == null || transaktionsnummer.Length == 0)
                {
                    return;
                }
                StreamWriter file = new StreamWriter(FILE_NAME_TRANSNR_STROM);
                file.WriteLine(transaktionsnummer);
                file.Close();

                if (ts.EinmaldatenSenden())
                {
                    log.Info("Einmaldaten gesendet, Unterbrechungen werden gesendet...");
                    if (ts.UnterbrechungenSenden())
                    {
                        log.Info("Unterbrechungen gesendet, Transaktion wird abgeschlossen.");
                        ts.TransaktionAbschliessen();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// Ermittelt das Ergebnis der Vorprüfung
        /// </summary>     
        private static void StromAntwortHolen()
        {
            StreamReader file = new StreamReader(FILE_NAME_TRANSNR_STROM);
            string transaktionsnummer = file.ReadLine();
            log.Info("Antwort holfen für Transaktionsnummer: " + transaktionsnummer);

            TransferStrom ts = new TransferStrom();
            ts.EinmaldatenEinlesen();
            string ret = ts.AntwortVorpruefungHolen(transaktionsnummer);
            log.Info("Ergebnis der Vorprüfung: " + ret);
        }

        /// <summary>
        /// Versand der Einmaldaten und Versorgungsunterbrechungen Gas
        /// </summary>
        private static void GasDatenSenden()
        {
            try
            {
                TransferGas tg = new TransferGas();
                tg.EinmaldatenEinlesen();
                tg.UnterbrechungenEinlesen();

                String transaktionsnummer = tg.TransaktionBeginnen();
                if (transaktionsnummer == null || transaktionsnummer.Length == 0)
                {
                    return;
                }
                StreamWriter file = new StreamWriter(FILE_NAME_TRANSNR_GAS);
                file.WriteLine(transaktionsnummer);
                file.Close();

                if (tg.EinmaldatenSenden())
                {
                    log.Info("Einmaldaten gesendet, Unterbrechungen werden gesendet...");
                    if (tg.UnterbrechungenSenden())
                    {
                        log.Info("Unterbrechungen gesendet, Transaktion wird abgeschlossen.");
                        tg.TransaktionAbschliessen();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// Ermittelt das Ergebnis der Vorprüfung
        /// </summary>
        private static void GasAntwortHolen()
        {
            StreamReader file = new StreamReader(FILE_NAME_TRANSNR_GAS);
            string transaktionsnummer = file.ReadLine();
            log.Info("Antwort holfen für Transaktionsnummer: " + transaktionsnummer);

            TransferGas tg = new TransferGas();

            tg.EinmaldatenEinlesen();
            string ret = tg.AntwortVorpruefungHolen(transaktionsnummer);
            log.Info("Ergebnis der Vorprüfung: " + ret);
        }
    }
}
