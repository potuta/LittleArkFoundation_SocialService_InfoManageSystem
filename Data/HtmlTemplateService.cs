using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace LittleArkFoundation.Data
{
    public class HtmlTemplateService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ConnectionService _connectionService;

        public HtmlTemplateService(IWebHostEnvironment environment, ConnectionService connectionService)
        {
            _environment = environment;
            _connectionService = connectionService;
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page1(string htmlContent, string dbType, int id)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var assessment = await context.Assessments.FirstOrDefaultAsync(a => a.PatientID == id);
            var referral = await context.Referrals.FirstOrDefaultAsync(r => r.PatientID == id);
            var informant = await context.Informants.FirstOrDefaultAsync(i => i.PatientID == id);

            if (patient == null)
            {
                return null;
            }

            // UPDATE LOGO IMAGE
            string imagePath = Path.Combine(_environment.WebRootPath, "resources", "NCH-Logo.png");
            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            htmlContent = htmlContent.Replace("/resources/NCH-Logo.png", $"data:image/png;base64,{base64String}");

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // ASSESSMENTS
            var dateofinterview = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Dateofinterview']");
            if (dateofinterview != null)
            {
                dateofinterview.InnerHtml = assessment.DateOfInterview.ToString();
            }

            var timeofinterview = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Timeofinterview']");
            if (timeofinterview != null)
            {
                timeofinterview.InnerHtml = assessment.TimeOfInterview.ToString();
            }

            var basicward = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Basicward']");
            if (basicward != null)
            {
                basicward.InnerHtml = assessment.BasicWard;
            }

            var nonbasicward = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonbasicward']");
            if (nonbasicward != null)
            {
                nonbasicward.InnerHtml = assessment.NonBasicWard;
            }

            var healthrecordno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Healthrecord']");
            if (healthrecordno != null)
            {
                healthrecordno.InnerHtml = assessment.HealthRecordNo;
            }

            var mswdno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Mswdno']");
            if (mswdno != null)
            {
                mswdno.InnerHtml = assessment.MSWDNo;
            }

            // REFERRALS
            var referralname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Namereferral']");
            if (referralname != null)
            {
                referralname.InnerHtml = referral.Name;
            }

            var referraladdress = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Addressreferral']");
            if (referraladdress != null)
            {
                referraladdress.InnerHtml = referral.Address;
            }

            var referralcontactno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Contactnoreferral']");
            if (referralcontactno != null)
            {
                referralcontactno.InnerHtml = referral.ContactNo;
            }

            // INFORMANTS
            var informantname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Informantname']");
            if (informantname != null)
            {
                informantname.InnerHtml = informant.Name;
            }

            var relationtopatient = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Relationtopatient']");
            if (relationtopatient != null)
            {
                relationtopatient.InnerHtml = informant.RelationToPatient;
            }

            var informantcontactno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Contactnoinformant']");
            if (informantcontactno != null)
            {
                informantcontactno.InnerHtml = informant.ContactNo;
            }

            var informantaddress = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Addressinformant']");
            if (informantaddress != null)
            {
                informantaddress.InnerHtml = informant.Address;
            }

            // PATIENTS
            var patientlastname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientsurname']");
            if (patientlastname != null)
            {
                patientlastname.InnerHtml = patient.LastName;
            }

            var patientfirstname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientfirstname']");
            if (patientfirstname != null)
            {
                patientfirstname.InnerHtml = patient.FirstName;
            }

            var patientmiddlename = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientmiddlename']");
            if (patientmiddlename != null)
            {
                patientmiddlename.InnerHtml = patient.MiddleName;
            }

            var patientdateofbirth = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Dateofbirth']");
            if (patientdateofbirth != null)
            {
                patientdateofbirth.InnerHtml = patient.DateOfBirth.ToString();
            }

            var patientage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientage']");
            if (patientage != null)
            {
                patientage.InnerHtml = patient.Age.ToString();
            }

            switch (patient.Sex)
            {
                case "Male":
                    var sexmalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sexmalecheckbox']");
                    if (sexmalecheckbox != null)
                    {
                        string existingStyle = sexmalecheckbox.GetAttributeValue("style", "");
                        sexmalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Female":
                    var sexfemalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sexfemalecheckbox']");
                    if (sexfemalecheckbox != null)
                    {
                        string existingStyle = sexfemalecheckbox.GetAttributeValue("style", "");
                        sexfemalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var patientcontactno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Contactnopatient']");
            if (patientcontactno != null)
            {
                patientcontactno.InnerHtml = patient.ContactNo;
            }

            var patientplaceofbirth = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Placeofbirth']");
            if (patientplaceofbirth != null)
            {
                patientplaceofbirth.InnerHtml = patient.PlaceOfBirth;
            }

            switch (patient.Gender)
            {
                case "Male":
                    var gendermalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Gendermalecheckbox']");
                    if (gendermalecheckbox != null)
                    {
                        string existingStyle = gendermalecheckbox.GetAttributeValue("style", "");
                        gendermalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Female":
                    var genderfemalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Genderfemalecheckbox']");
                    if (genderfemalecheckbox != null)
                    {
                        string existingStyle = genderfemalecheckbox.GetAttributeValue("style", "");
                        genderfemalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "LGBTQIA+":
                    var genderlgbtqcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Genderlgbtqcheckbox']");
                    if (genderlgbtqcheckbox != null)
                    {
                        string existingStyle = genderlgbtqcheckbox.GetAttributeValue("style", "");
                        genderlgbtqcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var patientreligion = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientreligion']");
            if (patientreligion != null)
            {
                patientreligion.InnerHtml = patient.Religion;
            }

            var patientnationality = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientnationality']");
            if (patientnationality != null)
            {
                patientnationality.InnerHtml = patient.Nationality;
            }

            var permanentaddress = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Permanentaddress']");
            if (permanentaddress != null)
            {
                permanentaddress.InnerHtml = patient.PermanentAddress;
            }

            var temporaryaddress = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Temporaryaddress']");
            if (temporaryaddress != null)
            {
                temporaryaddress.InnerHtml = patient.TemporaryAddress;
            }

            switch (patient.CivilStatus)
            {
                case "Legitimate":
                    var civillegitimatecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Legitimatecheckbox']");
                    if (civillegitimatecheckbox != null)
                    {
                        string existingStyle = civillegitimatecheckbox.GetAttributeValue("style", "");
                        civillegitimatecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Illegitimate":
                    var civilillegitimatecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Illegitimatecheckbox']");
                    if (civilillegitimatecheckbox != null)
                    {
                        string existingStyle = civilillegitimatecheckbox.GetAttributeValue("style", "");
                        civilillegitimatecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Married":
                    var civilmarriedcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Marriedcheckbox']");
                    if (civilmarriedcheckbox != null)
                    {
                        string existingStyle = civilmarriedcheckbox.GetAttributeValue("style", "");
                        civilmarriedcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Common Law":
                    var civilcommonlawcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Commonlawcheckbox']");
                    if (civilcommonlawcheckbox != null)
                    {
                        string existingStyle = civilcommonlawcheckbox.GetAttributeValue("style", "");
                        civilcommonlawcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Widowed":
                    var civilwidowedcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Widowedcheckbox']");
                    if (civilwidowedcheckbox != null)
                    {
                        string existingStyle = civilwidowedcheckbox.GetAttributeValue("style", "");
                        civilwidowedcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (patient.EducationLevel)
            {
                case "Primary":
                    var primarycheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Primarycheckbox']");
                    if (primarycheckbox != null)
                    {
                        string existingStyle = primarycheckbox.GetAttributeValue("style", "");
                        primarycheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Secondary":
                    var secondarycheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Secondarycheckbox']");
                    if (secondarycheckbox != null)
                    {
                        string existingStyle = secondarycheckbox.GetAttributeValue("style", "");
                        secondarycheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Vocational":
                    var vocationalcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Vocationalcheckbox']");
                    if (vocationalcheckbox != null)
                    {
                        string existingStyle = vocationalcheckbox.GetAttributeValue("style", "");
                        vocationalcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Tertiary":
                    var tertiarycheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Tertiarycheckbox']");
                    if (tertiarycheckbox != null)
                    {
                        string existingStyle = tertiarycheckbox.GetAttributeValue("style", "");
                        tertiarycheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "No Educational Attainment":
                    var noeducationcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Noeducationcheckbox']");
                    if (noeducationcheckbox != null)
                    {
                        string existingStyle = noeducationcheckbox.GetAttributeValue("style", "");
                        noeducationcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var patientoccupation = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientoccupation']");
            if (patientoccupation != null)
            {
                patientoccupation.InnerHtml = patient.Occupation;
            }

            var patientmonthlyincome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientsmonthlyincome']");
            if (patientmonthlyincome != null)
            {
                patientmonthlyincome.InnerHtml = patient.MonthlyIncome.ToString();
            }

            var philhealthpin = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Pin']");
            if (philhealthpin != null)
            {
                philhealthpin.InnerHtml = patient.PhilhealthPIN.ToString();
            }

            switch (patient.PhilhealthMembership)
            {
                case "Direct Contributor":
                    var directcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Directcontributorcheckbox']");
                    if (directcheckbox != null)
                    {
                        string existingStyle = directcheckbox.GetAttributeValue("style", "");
                        directcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Indirect Contributor":
                    var indirectcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Indirectcontributorcheckbox']");
                    if (indirectcheckbox != null)
                    {
                        string existingStyle = indirectcheckbox.GetAttributeValue("style", "");
                        indirectcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            // FAMILY COMPOSITION


            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page2(string htmlContent, string dbType, int id)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);

            if (patient == null)
            {
                return null;
            }

            htmlContent = htmlContent.Replace("{FullName}", patient.FirstName)
                                     .Replace("{Email}", patient.ContactNo)
                                     .Replace("{Address}", patient.PermanentAddress)
                                     .Replace("{Message}", patient.PhilhealthMembership);

            return htmlContent;
        }
    }
}
