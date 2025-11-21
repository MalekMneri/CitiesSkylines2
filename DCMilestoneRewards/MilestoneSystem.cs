using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Prefabs;
using Unity.Collections;
using Unity.Entities;

namespace DCMilestoneRewards
{
	internal partial class MilestoneSystem : GameSystemBase
	{
		private Setting settings;
		private EntityQuery milestoneQuery;

		private int[] originalMilestoneRewards;

		protected override void OnCreate()
		{
			base.OnCreate();
            this.milestoneQuery = GetEntityQuery(ComponentType.ReadOnly<MilestoneData>());
            this.settings = Mod.INSTANCE.settings();

            settings.onSettingsApplied += setting =>
            {
                if (setting.GetType() == typeof(Setting))
                {
                    this.updateGlobal((Setting)setting);
                }
            };
        }

        protected override void OnGameLoaded(Context serializationContext)
        {
            base.OnGameLoaded(serializationContext);

            this.originalMilestoneRewards = this.cacheMilestoneRewards();

            this.updateGlobal(settings);
        }

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
		}

		private void updateGlobal(Setting settings)
		{
			this.updateMilestoneRewards(!settings.disableMilestoneRewards);
		}

		protected override void OnUpdate()
		{
			
		}

		private int[] cacheMilestoneRewards()
		{
			NativeArray<MilestoneData> nativeArray2 = this.milestoneQuery.ToComponentDataArray<MilestoneData>(Allocator.Temp);
			int[] rewards = new int[nativeArray2.Length];
			try
			{
				for (int i = 0; i < nativeArray2.Length; i++)
				{
					rewards[i] = nativeArray2[i].m_Reward;
				}
			}
			finally
			{
				nativeArray2.Dispose();
			}

			return rewards;
		}

		private void updateMilestoneRewards(bool toggle)
		{
			NativeArray<Entity> nativeArray = this.milestoneQuery.ToEntityArray(Allocator.Temp);
			NativeArray<MilestoneData> nativeArray2 = this.milestoneQuery.ToComponentDataArray<MilestoneData>(Allocator.Temp);

			try
			{
				for (int i = 0; i < nativeArray2.Length; i++)
				{
					var milestone = nativeArray2[i];

					if (toggle)
					{
						milestone.m_Reward = this.originalMilestoneRewards[i];
					}
					else
					{
						milestone.m_Reward = 0;
					}

                    Mod.log.Info("Updating milestone " + milestone.m_Index.ToString() + " to " + milestone.m_Reward.ToString());

                    nativeArray2[i] = milestone;

					var entity = nativeArray[i];
					EntityManager.SetComponentData(entity, milestone);
					EntityManager.AddComponent<BatchesUpdated>(entity);
				}
			}
			finally
			{
				nativeArray.Dispose();
				nativeArray2.Dispose();
			}
		}
	}
}
