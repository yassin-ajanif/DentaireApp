using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Validation;

public static class TreatmentInfoValidator
{
    public static void Validate(TreatmentInfo treatmentInfo)
    {
        if (treatmentInfo.PrixConven != treatmentInfo.Recu + treatmentInfo.ARecevoir)
        {
            throw new InvalidOperationException("Prix Conven must equal Recu + A Recevoir.");
        }
    }
}

