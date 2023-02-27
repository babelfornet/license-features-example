using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Babel.Licensing;

namespace LicenseFeatures;

public partial class MainForm : Form
{
    private LicenseManager LM;

    [Obfuscation]
    enum LicenseType {
        Standard,
        Professional,
        Enterprise
    }

    [Obfuscation]
    class LicenseEdition
    {
        public string Name { get; set; }
        public LicenseType Type { get; set; }
    }

    public MainForm()
    {
        InitializeComponent();
        LM = LicenseManager.Instance;

        dateTimeExpireDate.Value = DateTime.Now.AddDays(30);
        dateTimeExpireDate.Enabled = checkBoxExpireDate.Checked;
        numericExpireDays.Enabled = checkBoxExpireDays.Checked;

        buttonFeature1.Enabled = false;
        buttonFeature2.Enabled = false;
        buttonFeature3.Enabled = false;

        comboBoxType.DataSource = new LicenseEdition[] {
            new LicenseEdition() { Name = "Standard", Type = LicenseType.Standard },
            new LicenseEdition() { Name = "Professional", Type = LicenseType.Professional },
            new LicenseEdition() { Name = "Enterprise", Type = LicenseType.Enterprise },
        };

        comboBoxType.SelectedItem = LicenseType.Standard;
        CheckLicenseFile();
    }

    #region Event Handlers
    private void checkBoxExpireDate_CheckedChanged(object sender, EventArgs e)
    {
        dateTimeExpireDate.Enabled = checkBoxExpireDate.Checked;
    }

    private void checkBoxExpireDays_CheckedChanged(object sender, EventArgs e)
    {
        numericExpireDays.Enabled = checkBoxExpireDays.Checked;
    }

    private void buttonCreate_Click(object sender, EventArgs e)
    {
        try
        {
            CreateLicense();
            CheckLicenseFile();
        }
        catch (Exception ex)
        {
            labelMessage.Text = ex.Message;
        }
    }

    private void CheckLicenseFile()
    {
        buttonValidate.Enabled = File.Exists(LM.LicenseFileName);
    }

    private void buttonValidate_Click(object sender, EventArgs e)
    {
        try
        {
            ValidateLicense();
        }
        catch (Exception ex)
        {
            labelMessage.Text = ex.Message;
        }
    }

    private void buttonFeature1_Click(object sender, EventArgs e)
    {
        try
        {
            OnFeature1();
        }
        catch (Exception ex)
        {
            SetErrorMessage(labelFeature1, ex);
        }
    }

    private void buttonFeature2_Click(object sender, EventArgs e)
    {
        try
        {
            OnFeature2();
        }
        catch (Exception ex)
        {
            SetErrorMessage(labelFeature2, ex);
        }
    }

    private void buttonFeature3_Click(object sender, EventArgs e)
    {
        try
        {
            OnFeature3();
        }
        catch (Exception ex) 
        { 
            SetErrorMessage(labelFeature3, ex); 
        }
    }
    #endregion

    private void CreateLicense()
    {
        string licenseId = textBoxLicenseId.Text;
        DateTime? expireDate = checkBoxExpireDate.Checked ? dateTimeExpireDate.Value : (DateTime?)null;
        int? expireDays = checkBoxExpireDays.Checked ? Decimal.ToInt32(numericExpireDays.Value) : (int?)null;

        LicenseEdition licenseEdition = comboBoxType.SelectedItem as LicenseEdition;

        XmlLicense license = new XmlLicense();
        license.ForAssembly(typeof(MainForm).Assembly)
               .WithId(licenseId)
               .WithType(licenseEdition.Name)
               .ExpiresAt(expireDate)
               .WithTrialDays(expireDays);

        if (licenseEdition.Type == LicenseType.Standard || licenseEdition.Type == LicenseType.Professional || licenseEdition.Type == LicenseType.Enterprise)
            license.WithFeature("feature1", string.Empty, Encoding.UTF8.GetBytes("aR&tj4i7u@"));

        if (licenseEdition.Type == LicenseType.Professional || licenseEdition.Type == LicenseType.Enterprise)
            license.WithFeature("feature2", string.Empty, Encoding.UTF8.GetBytes("q4oZ7Y%pk0"));

        if (licenseEdition.Type == LicenseType.Enterprise)
            license.WithFeature("feature3", string.Empty, Encoding.UTF8.GetBytes("U1#3ln2$vx"));

        LM.SaveLicense(license);

        labelMessage.Text = $"{licenseEdition.Name} licese created successfully";
    }

    private void ValidateLicense()
    {
        var license = LM.ValidateLicense();
        labelMessage.Text = LM.GetLicenseInfo(license);

        listBoxFeatures.Items.Clear();
        foreach (var feature in license.Features)
            listBoxFeatures.Items.Add(feature.Name);

        buttonFeature1.Enabled = true;
        buttonFeature2.Enabled = true;
        buttonFeature3.Enabled = true;

        buttonFeature1.Text = "Call Feature1 (" + (LM.HasFeature("feature1") ? "" : "not ") + "availalbe)";
        buttonFeature2.Text = "Call Feature2 (" + (LM.HasFeature("feature2") ? "" : "not ") + "availalbe)";
        buttonFeature3.Text = "Call Feature3 (" + (LM.HasFeature("feature3") ? "" : "not ") + "availalbe)";
    }

    private void OnFeature1()
    {
        var feature = new Feature1();
        SetMessage(labelFeature1, feature.DoTask());
    }

    private void OnFeature2()
    {
        var feature = new Feature2();
        SetMessage(labelFeature2, feature.DoTask());
    }

    private void OnFeature3()
    {
        var feature = new Feature3();
        SetMessage(labelFeature3, feature.DoTask());
    }

    private void SetErrorMessage(Label label, Exception ex)
    {
        string message = ex.Message;
        if (ex.InnerException != null)
            message = ex.InnerException.Message;

        label.Text = message;
        label.BackColor = Color.FromArgb(0xFF, 0x23, 0x23);
    }

    private void SetMessage(Label label, string text)
    {
        label.Text = text;
        label.BackColor = Color.FromArgb(0x44, 0xFF, 0x44);
    }
}
