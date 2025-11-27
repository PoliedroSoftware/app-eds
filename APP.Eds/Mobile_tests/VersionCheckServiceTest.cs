namespace Mobile_tests;

/// <summary>
/// Tests for version comparison logic used in version checking
/// </summary>
[TestFixture]
public class VersionCheckServiceTest
{
    [Test]
    [Category("VersionCheck")]
    public void CompareVersions_SameVersions_ReturnsZero()
    {
        // Arrange
        var version1 = "1.0.1";
        var version2 = "1.0.1";

        // Act
        var result = CompareVersions(version1, version2);

        // Assert
        Assert.That(result, Is.EqualTo(0), "Same versions should return 0");
    }

    [Test]
    [Category("VersionCheck")]
    public void CompareVersions_FirstVersionLower_ReturnsNegative()
    {
        // Arrange
        var version1 = "1.0.0";
        var version2 = "1.0.1";

        // Act
        var result = CompareVersions(version1, version2);

        // Assert
        Assert.That(result, Is.LessThan(0), "Lower version should return negative value");
    }

    [Test]
    [Category("VersionCheck")]
    public void CompareVersions_FirstVersionHigher_ReturnsPositive()
    {
        // Arrange
        var version1 = "1.0.2";
        var version2 = "1.0.1";

        // Act
        var result = CompareVersions(version1, version2);

        // Assert
        Assert.That(result, Is.GreaterThan(0), "Higher version should return positive value");
    }

    [Test]
    [Category("VersionCheck")]
    public void CompareVersions_MajorVersionDifferent_ReturnsCorrectResult()
    {
        // Arrange
        var version1 = "1.0.1";
        var version2 = "2.0.0";

        // Act
        var result = CompareVersions(version1, version2);

        // Assert
        Assert.That(result, Is.LessThan(0), "Lower major version should return negative value");
    }

    [Test]
    [Category("VersionCheck")]
    public void CompareVersions_MinorVersionDifferent_ReturnsCorrectResult()
    {
        // Arrange
        var version1 = "1.2.0";
        var version2 = "1.3.0";

        // Act
        var result = CompareVersions(version1, version2);

        // Assert
        Assert.That(result, Is.LessThan(0), "Lower minor version should return negative value");
    }

    [Test]
    [Category("VersionCheck")]
    public void CompareVersions_DifferentLengths_ComparesCorrectly()
    {
        // Arrange
        var version1 = "1.0";
        var version2 = "1.0.1";

        // Act
        var result = CompareVersions(version1, version2);

        // Assert
        Assert.That(result, Is.LessThan(0), "1.0 should be less than 1.0.1");
    }

    [Test]
    [Category("VersionCheck")]
    public void CompareVersions_DifferentLengthsSameValue_ReturnsZero()
    {
        // Arrange
        var version1 = "1.0.0";
        var version2 = "1.0";

        // Act
        var result = CompareVersions(version1, version2);

        // Assert
        Assert.That(result, Is.EqualTo(0), "1.0.0 should equal 1.0");
    }

    /// <summary>
    /// Compares two version strings (copied from VersionCheckService for testing)
    /// </summary>
    private int CompareVersions(string version1, string version2)
    {
        var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
        var v2Parts = version2.Split('.').Select(int.Parse).ToArray();

        int maxLength = Math.Max(v1Parts.Length, v2Parts.Length);
        
        for (int i = 0; i < maxLength; i++)
        {
            int v1Part = i < v1Parts.Length ? v1Parts[i] : 0;
            int v2Part = i < v2Parts.Length ? v2Parts[i] : 0;

            if (v1Part < v2Part) return -1;
            if (v1Part > v2Part) return 1;
        }

        return 0;
    }
}
