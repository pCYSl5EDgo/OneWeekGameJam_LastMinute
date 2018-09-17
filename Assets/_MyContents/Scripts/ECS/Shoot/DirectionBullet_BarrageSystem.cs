using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace MainContents.ECS
{
    public sealed class DirectionBullet_BarrageSystem : ComponentSystem
    {
        struct Group
        {
            public int Length;
            public ComponentDataArray<EnemyData> EnemyData;
            [ReadOnly] public ComponentDataArray<Position> Position;
            [ReadOnly] public SharedComponentDataArray<BarrageSettings_DirectionBullet> Settings;
        }

        struct PlayerGroup
        {
            public int Length;
            [ReadOnly] public ComponentDataArray<Position> Position;
            [ReadOnly] public ComponentDataArray<PlayerInput> Input;
            [ReadOnly] public SharedComponentDataArray<PlayerSettings> Settings;
        }

        [Inject] Group _group;
        [Inject] PlayerGroup _playerGroup;

        protected override void OnUpdate()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < this._group.Length; i++)
            {
                var data = this._group.EnemyData[i];
                var pos = this._group.Position[i];
                var settings = this._group.Settings[i];
                data.CooldownTimeCounter -= deltaTime;
                if (data.CooldownTimeCounter <= 0f)
                {
                    data.CooldownTimeCounter = settings.CommonBarrageSettings.CooldownTime;
                    this.SpawnBullet(ref pos, ref settings);
                }
                this._group.EnemyData[i] = data;
            }
        }

        // 敵の生成
        void SpawnBullet(ref Position pos, ref BarrageSettings_DirectionBullet settings)
        {
            if (this._playerGroup.Position.Length <= 0) { return; }

            PostUpdateCommands.CreateEntity(MainECS_Manager.EnemyBulletArchetype);
            PostUpdateCommands.SetComponent(pos);
            PostUpdateCommands.SetComponent(
                new BulletData
                {
                    ShotSpeed = settings.CommonBarrageSettings.ShotSpeed,
                    ShotAngle = this.Aiming(pos.Value, this._playerGroup.Position[0].Value),
                    Lifespan = settings.CommonBarrageSettings.Lifespan
                });
            PostUpdateCommands.AddSharedComponent(MainECS_Manager.EnemyBulletLook);
            PostUpdateCommands.AddSharedComponent(MainECS_Manager.BulletCollision);
        }

        float Aiming(float3 p1, float3 p2)
        {
            float dx = p2.x - p1.x;
            float dy = p2.z - p1.z;
            float rad = math.atan2(dy, dx);
            return math.degrees(rad);
        }
    }
}