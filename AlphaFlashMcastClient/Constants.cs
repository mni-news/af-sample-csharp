using System;
using System.Collections.Generic;
using System.Text;

namespace AlphaFlashCSharpMcastClient
{
    public class Constants
    {
        public const int HEADER_SIZE = 10; // Size of header on the wire
        public const int CRC_SIZE = 4; // Size of trailing CRC on the wire

        public const int FLOAT_FIELD_TYPE = 0; // Id of float on the wire
        public const int FLOAT_FIELD_SIZE = 4; // Size of float on the wire
        public const int SHORT_FIELD_TYPE = 2; // Id of int on the wire
        public const int SHORT_FIELD_SIZE = 2; // Size of int on the wire
        public const int LONG_FIELD_TYPE = 3; // Id of long on the wire
        public const int LONG_FIELD_SIZE = 8; // Size of long on the wire
        public const int DOUBLE_FIELD_TYPE = 4; // Id of double on the wire
        public const int DOUBLE_FIELD_SIZE = 8; // Size of double on the wire
        public const int BOOL_FIELD_TYPE = 6; // Id of boolean on the wire
        public const int BOOL_FIELD_SIZE = 1; // Size of boolean on the wire
        public const int YES_NO_NA_FIELD_TYPE = 7; // Id of yes_no_na on the wire
        public const int YES_NO_NA_FIELD_SIZE = 1; // Size of yes_no_na on the wire
        public const int DIRECTIONAL_FIELD_TYPE = 8; // Id of directional on the wire
        public const int DIRECTIONAL_FIELD_SIZE = 1; // Size of directional on the wire
        public const int INT_FIELD_TYPE = 9; // Id of int on the wire
        public const int INT_FIELD_SIZE = 4; // Size of int on the wire

        public const int FLOAT_INDICATOR_SIZE = FLOAT_FIELD_SIZE + 2; // Size of type/id/float on the wire
        public const int SHORT_INDICATOR_SIZE = SHORT_FIELD_SIZE + 2; // Size of type/id/short on the wire	  
        public const int LONG_INDICATOR_SIZE = LONG_FIELD_SIZE + 2; // Size of type/id/long on the wire	  
        public const int DOUBLE_INDICATOR_SIZE = DOUBLE_FIELD_SIZE + 2; // Size of type/id/double on the wire	  
        public const int BOOL_INDICATOR_SIZE = BOOL_FIELD_SIZE + 2; // Size of type/id/boolean on the wire	  
        public const int YES_NO_NA_INDICATOR_SIZE = YES_NO_NA_FIELD_SIZE + 2; // Size of type/id/yes_no_na on the wire	  
        public const int DIRECTIONAL_INDICATOR_SIZE = DIRECTIONAL_FIELD_SIZE + 2; // Size of type/id/directional on the wire	  	  
        public const int INT_INDICATOR_SIZE = INT_FIELD_SIZE + 2; // Size of type/id/int on the wire	
    }
}
