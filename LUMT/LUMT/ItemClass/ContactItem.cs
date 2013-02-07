// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using LUMT.Enum;
namespace LUMT.ItemClass
{
    class ContactItem
    {
        private string contactUri;

        public string ContactUri
        {
            get { return contactUri; }
        }

        private ContactType contactType;

        public ContactType ContactType
        {
            get { return contactType; }
        }

        public ContactItem(string contactUri, ContactType contactType)
        {
            this.contactUri = contactUri;
            this.contactType = contactType;
        }
    }
}
