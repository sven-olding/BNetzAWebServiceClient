using FileHelpers;
using System;

namespace BNetzAWebServiceClient
{
    [DelimitedRecord(";")]
    internal class EinmaldatenGas
    {
        [FieldConverter(ConverterKind.Decimal, ",")]
        [FieldNullValue(typeof(Decimal), "0")]
        public decimal A_qu_kg1;

        [FieldConverter(ConverterKind.Decimal, ",")]
        [FieldNullValue(typeof(Decimal), "0")]
        public decimal A_qu_kg2;

        [FieldConverter(ConverterKind.Decimal, ",")]
        public decimal AnzahlVUGesamt;

        public int Berichtsjahr;

        public int Bnr;

        public bool Leermeldung;
        [FieldNullValue(0)]
        public int Nnr;

        public string VUG_Ansprech;
        [FieldNullValue(0)]
        public int VUG_ANZ_LV;

        public string VUG_Bericht_Text;
        [FieldNullValue(0)]
        public int VUG_Dopplung;

        public string VUG_Kommentar_Text;

        public string VUG_Kontakt;
        [FieldNullValue(0)]
        public int VUG_NB_Art;
        [FieldNullValue(0)]
        public int VUG_SUM_KAP_NACH;
        [FieldNullValue(0)]
        public int VUG_SUM_KAP_VOR;
        [FieldNullValue(0)]
        public int VUG_SUM_LST;
    }
}
