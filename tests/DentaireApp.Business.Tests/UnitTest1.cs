using DentaireApp.Business.Models.Patients;
using DentaireApp.Business.Validation;

namespace DentaireApp.Business.Tests;

public class UnitTest1
{
    [Fact]
    public void Validate_Throws_WhenFinancialEquationIsInvalid()
    {
        var sheet = new TreatmentSheet
        {
            Lines =
            [
                new TreatmentLine
                {
                    PrixConven = 100m,
                    Recu = 60m,
                    ARecevoir = 30m,
                },
            ],
        };

        Assert.Throws<InvalidOperationException>(() => TreatmentSheetValidator.Validate(sheet));
    }
}
