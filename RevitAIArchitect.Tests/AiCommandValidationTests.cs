using RevitAIArchitect;
using Xunit;

namespace RevitAIArchitect.Tests
{
    public class AiCommandValidationTests
    {
        [Fact]
        public void Rejects_Unknown_Action()
        {
            var cmd = new AiCommand { Action = "foo" };
            var (ok, err) = cmd.Validate();
            Assert.False(ok);
            Assert.Contains("Unknown action", err);
        }

        [Fact]
        public void Requires_ElementIds_For_Modifying_Actions()
        {
            var cmd = new AiCommand { Action = "delete" };
            var (ok, err) = cmd.Validate();
            Assert.False(ok);
            Assert.Contains("Element IDs", err);
        }

        [Fact]
        public void Requires_Parameter_And_Value_For_SetParameter()
        {
            var cmd = new AiCommand { Action = "set_parameter", ElementIds = new() { 1 } };
            var (ok, err) = cmd.Validate();
            Assert.False(ok);
            Assert.Contains("Parameter name", err);
        }

        [Fact]
        public void Accepts_Valid_OverrideColor_Hex()
        {
            var cmd = new AiCommand { Action = "override_color", ElementIds = new() { 1 }, Value = "#FF0000" };
            var (ok, _) = cmd.Validate();
            Assert.True(ok);
        }

        [Fact]
        public void Rejects_Invalid_Color_Format()
        {
            var cmd = new AiCommand { Action = "override_color", ElementIds = new() { 1 }, Value = "red" };
            var (ok, err) = cmd.Validate();
            Assert.False(ok);
            Assert.Contains("Color value", err);
        }

        [Fact]
        public void Requires_ViewId_For_OpenView()
        {
            var cmd = new AiCommand { Action = "open_view", Value = "abc" };
            var (ok, err) = cmd.Validate();
            Assert.False(ok);
            Assert.Contains("View ID", err);
        }
    }
}
