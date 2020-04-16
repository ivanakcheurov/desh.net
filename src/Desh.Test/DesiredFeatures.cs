using System.Threading.Tasks;
using Xunit;

namespace Desh.Test
{
    public class DesiredFeatures
    {
        private const string DecideIf = @"
decide: diesel
if:
    vehicle_type:
        - van
        - cruise_ship
";
        private const string DecideExtend = @"
vehicle_type:
    - van:
        decide: diesel
        extend:
            has_rear_camera: yes
            decide: safe
";
        

        [Theory(Skip = "Not implemented")]
        [InlineData(DecideIf, "{vehicle_type: 'van'}", "diesel")]
        [InlineData(DecideIf, "{vehicle_type: 'cruise_ship'}", "diesel")]
        [InlineData(DecideIf, "{vehicle_type: 'jet'}", null)]
        [InlineData(DecideExtend, "{vehicle_type: 'van', has_rear_camera: 'yes'}", "['diesel', 'safe']")]
        [InlineData(DecideExtend, "{vehicle_type: 'van', has_rear_camera: 'no'}", "['diesel']")]
        [InlineData(DecideExtend, "{vehicle_type: 'jet', has_rear_camera: 'yes'}", null)] // kerosene
        [InlineData(DecideExtend, "{vehicle_type: 'jet', has_rear_camera: 'no'}", null)]
        public async Task Picks_correct_decision(string desh, string contextJson, string expectedDecision)
        {
            await ParseExecuteTestRunner.AssertPicksCorrectDecision(desh, contextJson, expectedDecision);
        }
    }
}
