using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

using MainContents.ScriptableObjects;

namespace MainContents.ECS
{
    /// <summary>
    /// 敵の生成処理
    /// </summary>
    public sealed class EnemySpawnSystem : ComponentSystem
    {
        struct Group
        {
            public readonly int Length;
            public ComponentDataArray<EnemySpawnSystemData> Data;
            [ReadOnly] public SharedComponentDataArray<EnemySpawnSystemSettings> Settings;
        }

        [Inject] Group _group;

        protected override void OnUpdate()
        {
            float deltaTime = Time.deltaTime;
            var data = this._group.Data[0];
            var spawnSettings = this._group.Settings[0];
            data.CooldownTimeCounter -= deltaTime;

            if (data.CooldownTimeCounter <= 0f && MainECS_Manager.CurrentFps > spawnSettings.LimitFps)
            {
                data.CooldownTimeCounter = spawnSettings.CooldownTime;
                this.SpawnEnemy(ref data, ref spawnSettings);
            }
            this._group.Data[0] = data;
        }

        // 敵の生成
        void SpawnEnemy(ref EnemySpawnSystemData data, ref EnemySpawnSystemSettings spawnSettings)
        {
            ++data.SpawnedEnemyCount;

            var type = UnityEngine.Random.Range(0, spawnSettings.MaxBarrageType);
            var pos = spawnSettings.RandomArea();

            var postUpdateCommands = PostUpdateCommands;
            postUpdateCommands.CreateEntity(MainECS_Manager.CommonEnemyArchetype);
            postUpdateCommands.SetComponent(new Position { Value = new float3(pos.x, 0, pos.y) });
            postUpdateCommands.SetComponent(new EnemyData { });
            postUpdateCommands.SetSharedComponent(MainECS_Manager.EnemyLook);
            postUpdateCommands.SetSharedComponent(MainECS_Manager.EnemyCollision);

            switch ((BarrageType)type)
            {
                case BarrageType.CircularBullet:
                    postUpdateCommands.AddSharedComponent(MainECS_Manager.BarrageSettings_CircularBullet);
                    break;
                case BarrageType.DirectionBullet:
                    postUpdateCommands.AddSharedComponent(MainECS_Manager.BarrageSettings_DirectionBullet);
                    break;
            }
        }
    }
}