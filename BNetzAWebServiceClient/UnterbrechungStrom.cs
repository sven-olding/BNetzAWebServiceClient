using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace BNetzAWebServiceClient
{
    [DelimitedRecord(";")] 
    class UnterbrechungStrom
    {      
        public int LfdNr;

        public bool VSU_1_14;

        public bool VSU_1_15;

        public bool VSU_1_16;

        public bool VSU_1_17;

        [FieldConverter(ConverterKind.Date, "dd.MM.yyyy HH:mm:ss")] 
        public DateTime VSU_1_18;

        public int VSU_1_19;

        public string VSU_1_20;

        public string StoerungsAnlass;

        public string VSU_1_36;

        [FieldConverter(ConverterKind.Decimal, ",")]
        public decimal KTrafo;

        [FieldConverter(ConverterKind.Decimal, ",")]
        public decimal KTrafo_Produkt;

        [FieldConverter(ConverterKind.Decimal, ",")]
        public decimal NTrafo;

        [FieldConverter(ConverterKind.Decimal, ",")]
        public decimal NTrafo_Produkt;

        public int VSU_1_13;

        public string NetzebeneID;
    }
}
