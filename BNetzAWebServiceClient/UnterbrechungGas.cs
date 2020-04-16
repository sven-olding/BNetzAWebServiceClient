using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace BNetzAWebServiceClient
{
    [DelimitedRecord(";")]
    class UnterbrechungGas
    {      
        public int LfdNr;

        public int stAnlassID;

        public int UnterbrechungsArt;

        public string vug_ANM;

        public int vug_AUSM_EG1;

        public int vug_AUSM_EG2;

        [FieldConverter(ConverterKind.Date, "dd.MM.yyyy HH:mm:ss")]
        public DateTime vug_Beginndatum;

        [FieldConverter(ConverterKind.Date, "dd.MM.yyyy HH:mm:ss")]
        public DateTime vug_Beginnzeit;

        public int vug_Dauer;

        public int vug_EG;

        public string vug_ETSO;

        public int vug_KMIN_EG1;

        public int vug_KMIN_EG2;

        public string vug_nb_name;

        public string vug_NB_betroffen;
    }
}
