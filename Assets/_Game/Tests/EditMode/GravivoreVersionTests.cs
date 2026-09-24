using Gravivore.Core;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class GravivoreVersionTests
    {
        [Test]
        public void BootstrapVersion_UsesInitialVerticalSliceVersion()
        {
            Assert.AreEqual("0.1.0", GravivoreVersion.AppVersion);
            Assert.AreEqual(1, GravivoreVersion.AndroidVersionCode);
            Assert.AreEqual("com.gravivore.mobile", GravivoreVersion.AndroidPackageId);
        }
    }
}
