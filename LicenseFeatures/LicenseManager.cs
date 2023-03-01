using Babel.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LicenseFeatures
{
    internal class LicenseManager
    {
        public static readonly LicenseManager Instance = new LicenseManager();

        private static ILicense license;

        public ILicense License 
        { 
            get
            {
                if (license == null)
                    license = ValidateLicense();

                return license;
            }
        }

        public string LicenseFileName
        {
            get
            {
                var asm = Assembly.GetAssembly(typeof(LicenseManager));
                return asm.GetName().Name + ".lic";
            }
        }

        public bool HasFeature(string name)
        {
            if (License == null) return false;
            return License.Features.Any(item => item.Name == name);
        }

        public byte[] GetFeature(string name)
        {
            if (License == null)
                return new byte[0];

            return License.Features.First(item => item.Name == name).Data;
        }

        public ILicense ValidateLicense()
        {
            // To validate the license only the public key is required.
            // The private key is only used to sign the license, and in production the private key
            // should not be distributed with the application.
            // The public key should be distributed with the application and used to validate the license.

            // The following signature provider can be used to create and validate the license
            // as includes both the private key and the public key.
            var signature = CreateSignature();

            // The public key string can be embedded into the application and used to create a signature
            // provider used only to validate the license.
            string publicKey = signature.ExportKeys(publicKeyOnly: true);

            // To create the RSASignature from the public key use the following code
            var validatingSignature = RSASignature.FromKeys(publicKey);

            // And assign the validating signature provider to the license manager
            var licenseManager = new XmlLicenseManager();
            licenseManager.SignatureProvider = validatingSignature;

            license = licenseManager.Validate(File.ReadAllText(LicenseFileName), typeof(LicenseManager), this);
            return license;
        }

        public void SaveLicense(ILicense license)
        {
            RSASignature signer = CreateSignature();

            license.SignWith(signer)
                   .Save(LicenseFileName);
        }

        public string GetLicenseInfo(ILicense license)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Id: {0}", license.Id);
            sb.AppendLine();
            sb.AppendFormat("Type: {0}", license.Type);
            sb.AppendLine();

            if (license.ExpireDate.HasValue)
            {
                sb.AppendFormat("Expire: {0}", license.ExpireDate);
                sb.AppendLine();
            }

            var trial = license.Restrictions.OfType<TrialRestriction>().FirstOrDefault();
            if (trial != null)
            {
                if (trial.ExpireDate.HasValue)
                {
                    sb.AppendFormat("Expire date: {0}", trial.ExpireDate);
                    sb.AppendLine();
                }

                if (trial.TimeLeft.HasValue)
                {
                    sb.AppendFormat("Trial time: {0}", trial.TimeLeft);
                    sb.AppendLine();
                }

                if (!string.IsNullOrEmpty(trial.Terms))
                {
                    sb.AppendFormat("Terms: {0}", trial.Terms);
                    sb.AppendLine();
                }
            }

            var hardware = license.Restrictions.OfType<HardwareRestriction>().FirstOrDefault();
            if (hardware != null)
                sb.AppendFormat("Hardware Key: {0}", hardware.HardwareKey);

            return sb.ToString();
        }

        private RSASignature CreateSignature()
        {           
            var rsa = new RSASignature();
            if (File.Exists("Keys.pem"))
            {
                return Pem.ReadSignature("Keys.pem") as RSASignature;
            }

            rsa.CreateKeyPair();
            rsa.WritePem("Keys.pem", false);

            return rsa;
        }

        // Returns the encrypted code password extracted from the license file
        [Obfuscation(Feature = "msil encryption get password")]
        internal static string GetSourcePassord(string source)
        {
            var lm = LicenseManager.Instance;

            var feature = lm.License.Features.FirstOrDefault(item => item.Name == source);
            if (feature == null)
                throw new ApplicationException(source + " not found");

            return Encoding.UTF8.GetString(feature.Data);
        }
    }
}
