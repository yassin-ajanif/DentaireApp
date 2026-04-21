using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Validation;

public static class TreatmentSheetValidator
{
    public static void Validate(TreatmentSheet sheet)
    {
        foreach (var line in sheet.Lines)
        {
            if (line.PrixConven != line.Recu + line.ARecevoir)
            {
                throw new InvalidOperationException("Prix Conven must equal Recu + A Recevoir.");
            }
        }
    }
}

