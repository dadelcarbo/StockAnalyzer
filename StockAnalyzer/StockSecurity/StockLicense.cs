using System;
using System.Collections.Generic;
using System.Text;

namespace StockAnalyzer.StockSecurity
{
    public class InvalidLicenseException : Exception
    {
        public InvalidLicenseException(string message)
           : base(message)
        {
        }
        public InvalidLicenseException(string message, System.Exception innerException)
           : base(message, innerException)
        {
        }
    }
    public class StockLicense
    {
        public DateTime ExpiryDate { get; private set; }
        public string UserID { get; private set; }
        public string MachineID { get; private set; }
        public string LicenseKey { get; private set; }
        public int MajorVerion { get; private set; }
        public int MinorVerion { get; private set; }
        public List<string> FeatureList { get; private set; }

        public StockLicense(string userID, string licenseKey)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] hash = enc.GetBytes(StockToolKit.GetHash(userID));

            byte[] formatedByteKey = StockToolKit.FromHexString(licenseKey);
            byte[] unformatedByteKey = new byte[formatedByteKey.Length];

            int i;
            for (i = 0; i < Math.Min(formatedByteKey.Length, hash.Length); i++)
            {
                unformatedByteKey[i] = (byte)(formatedByteKey[i] ^ hash[i]);
            }
            for (; i < unformatedByteKey.Length; i++)
            {
                unformatedByteKey[i] = formatedByteKey[i];
            }
            string unformatedLicenseKey = enc.GetString(unformatedByteKey);

            string[] fields = unformatedLicenseKey.Split('|');

            try
            {
                this.UserID = fields[userIDIndex];
                this.ExpiryDate = DateTime.Parse(fields[expriryDateIndex]);
                this.MajorVerion = int.Parse(fields[majorVersionIndex]);
                this.MinorVerion = int.Parse(fields[minorVersionIndex]);
                this.MachineID = fields[machineIDIndex];
                int count = int.Parse(fields[featureCountIndex]);
                this.FeatureList = new List<string>();
                for (i = 0; i < count; i++)
                {
                    this.FeatureList.Add(fields[i + featureListStartIndex]);
                }
            }
            catch (Exception e)
            {
                throw new System.Exception("Invalid license", e);
            }
        }
        public StockLicense(DateTime expiryDate, string userID, string machineID, List<string> featureList, int majorVersion, int minorVersion)
        {
            this.ExpiryDate = expiryDate;
            this.UserID = userID;
            this.MachineID = machineID;
            this.FeatureList = featureList;
            this.MajorVerion = majorVersion;
            this.MinorVerion = minorVersion;

            // Has to be the last line as it uses other fields
            this.LicenseKey = this.GenerateLicenseKey();
        }

        private static int userIDIndex = 0;
        private static int expriryDateIndex = 1;
        private static int majorVersionIndex = 2;
        private static int minorVersionIndex = 3;
        private static int machineIDIndex = 4;
        private static int featureCountIndex = 5;
        private static int featureListStartIndex = 6;

        private string GenerateLicenseKey()
        {
            string unformattedKey = this.UserID + "|" +
                this.ExpiryDate.ToShortDateString() + "|" +
                this.MajorVerion + "|" + this.MinorVerion + "|" +
                this.MachineID + "|" +
                this.FeatureList.Count;
            foreach (string feature in this.FeatureList)
            {
                unformattedKey += "|" + feature;
            }

            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] hash = enc.GetBytes(StockToolKit.GetHash(this.UserID));

            byte[] unformatedByteKey = enc.GetBytes(unformattedKey);
            byte[] formatedByteKey = new byte[unformatedByteKey.Length];

            int i = 0;
            for (i = 0; i < Math.Min(unformatedByteKey.Length, hash.Length); i++)
            {
                formatedByteKey[i] = (byte)(unformatedByteKey[i] ^ hash[i]);
            }
            for (; i < unformatedByteKey.Length; i++)
            {
                formatedByteKey[i] = unformatedByteKey[i];
            }

            return StockToolKit.GetHexString(formatedByteKey);
        }

    }
}
