using RoR2;
using RoR2.Achievements;

namespace Heretic
{
    //Give 10 coins so it covers the cost of purchasing the lunars.
    [RegisterAchievement("MoffeinHereticUnlock", "Survivors.MoffeinHeretic", null, 10u, null)]
    public class HereticUnlockAchievement : BaseEndingAchievement
    {

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("HereticBody");
        }
        public override bool ShouldGrant(RunReport runReport)
        {
            if (runReport.gameEnding && runReport.gameEnding.isWin && runReport.gameEnding == RoR2Content.GameEndings.MainEnding && this.localUser.cachedBody.bodyIndex == this.requiredBodyIndex)
            {
                return true;
            }
            return false;
        }
    }
}
