using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using log4net;
using log4net.Config;
using Bsc;
using System.Net.Security;

namespace BNetzAWebServiceClient
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private const string FILE_NAME_TRANSNR_STROM = "transaktionsnummer-strom.txt";

        private const string FILE_NAME_TRANSNR_GAS = "transaktionsnummer-gas.txt";

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(Certificates.ValidateRemoteCertificate);

            try
            {
                InitializeComandLineParser();
                ParseArguments(args);
            }
            catch (CommandLineArgumentException e)
            {
                log.Error(e.Message);
                PrintUsage();
                return;
            }
            
            bool antwortHolen = CommandLineArgumentParser.IsSwitchOn("-a");
            string type = CommandLineArgumentParser.GetParamValue("-type");
   
            log.Info("START");

            switch (type.ToUpper())
            {
                case "S":
                    if (antwortHolen)
                    {
                        StromAntwortHolen();
                    }
                    else
                    {
                        StromDatenSenden();
                    }
                    break;

                case "G":
                    if (antwortHolen)
                    {
                        GasAntwortHolen();
                    }
                    else
                    {
                        GasDatenSenden();
                    }
                    break;

                default:
                    PrintUsage();
                    break;
            }

            log.Info("ENDE");
        }

        /// <summary>
        /// Initialize available command line options
        /// </summary>
        private static void InitializeComandLineParser()
        {
            //
            // Define the required parameters.
            //
            string[] requiredArguments = { "-type" };
            CommandLineArgumentParser.DefineRequiredParameters(requiredArguments);

            //
            // Define the supported switches.
            //
            string[] switches = { "-a" };
            CommandLineArgumentParser.DefineSwitches(switches);
        }

        /// <summary>
        /// Parse command line options
        /// </summary>
        /// <param name="args">arguments from main method</param>
        private static void ParseArguments(string[] args)
        {
            //
            // Handle the special case "-help" separately
            //
            if (args.Length == 1 && args[0].Trim() == "-help")
            {
                PrintUsage();
            }
            else
            {
                CommandLineArgumentParser.ParseArguments(args);
            }
        }

        /// <summary>
        /// Print usage of command line
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("BNetzAWebServiceClient.exe -type <string> [-a]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  - type: Art der Übertragung, s für Strom g für Gas");
            Console.WriteLine("  - a: wenn angegeben werden keine Daten gesendet, nur die Antwort der Prüfung abgeholt");
            Console.WriteLine();
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
