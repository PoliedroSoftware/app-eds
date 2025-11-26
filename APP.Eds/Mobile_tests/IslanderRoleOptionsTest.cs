using APP.Eds.Services.Islander;

namespace Mobile_tests;

[TestFixture]
public class IslanderRoleOptionsTest
{
    [Test]
    [Category("Islander")]
    public void TestIslanderRoleOptions_ShouldOnlyContainOperario()
    {
        // Arrange
        var islanderService = new IslanderService();

        // Act
        var roleOptions = islanderService.RoleOptions;

        // Assert
        Assert.That(roleOptions.Count, Is.EqualTo(1), "RoleOptions should contain exactly one role");
        Assert.That(roleOptions[0], Is.EqualTo("Operario"), "The only role option should be 'Operario'");
    }

    [Test]
    [Category("Islander")]
    public void TestIslanderRoleOptions_ShouldNotContainOtherRoles()
    {
        // Arrange
        var islanderService = new IslanderService();
        var forbiddenRoles = new[] { "Supervisor", "Encargado de Turno", "Cajero", "Mantenimiento", "Seguridad" };

        // Act
        var roleOptions = islanderService.RoleOptions;

        // Assert
        foreach (var forbiddenRole in forbiddenRoles)
        {
            Assert.That(roleOptions, Does.Not.Contain(forbiddenRole), 
                $"RoleOptions should not contain '{forbiddenRole}'");
        }
    }
}
