// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using Microsoft.Rtc.Collaboration.Presence;
using LUMT.Enum;

namespace LUMT.ItemClass
{
    class ACEItem
    {
        private string itemValue;

        public string ItemValue
        {
            get { return itemValue; }
        }

        private ACEType aceType;

        public ACEType ACEType
        {
            get { return aceType; }
        }

        private PresenceRelationshipLevel presenceRelationshipLevel;

        public PresenceRelationshipLevel PresenceRelationshipLevel
        {
            get { return presenceRelationshipLevel; }
        }

        public ACEItem(ACEType aceType, string itemValue, PresenceRelationshipLevel presenceRelationshipLevel)
        {
            this.aceType = aceType;
            this.itemValue = itemValue;
            this.presenceRelationshipLevel = presenceRelationshipLevel;
        }
    }
}
