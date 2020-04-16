using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace BNetzAWebServiceClient
{
    [DelimitedRecord(";")]
    class EinmaldatenStrom
    {
        public int Berichtsjahr;

        public int Betriebsnummer;

        public string Kontrollnummer;

        public bool Leermeldung;

        public int Netznummer;

        public string VSU_1_11_1;

        [FieldConverter(ConverterKind.Decimal, ",")]
        [FieldNullValue(typeof(Decimal), "0")]
        public decimal VSU_1_11_2;

        [FieldNullValue(0)]
        public int VSU_1_2;

        [FieldConverter(ConverterKind.Decimal, ",")]
        [FieldNullValue(typeof(Decimal), "0")]
        public decimal VSU_1_3;

        [FieldConverter(ConverterKind.Decimal, ",")]
        [FieldNullValue(typeof(Decimal), "0")]
        public decimal VSU_1_4;

        [FieldConverter(ConverterKind.Decimal, ",")]
        [FieldNullValue(typeof(Decimal), "0")]
        public decimal VSU_1_5;

        [FieldConverter(ConverterKind.Decimal, ",")]
        [FieldNullValue(typeof(Decimal), "0")]
        public decimal VSU_1_6;

        [FieldConverter(ConverterKind.Decimal, ",")]
        [FieldNullValue(typeof(Decimal), "0")]
        public decimal VSU_1_7;

        [FieldConverter(ConverterKind.Decimal, ",")]
        [FieldNullValue(typeof(Decimal), "0")]
        public decimal VSU_1_8;

        public string VSU_1_9;
    }
}
