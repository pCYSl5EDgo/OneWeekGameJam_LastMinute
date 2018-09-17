using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace MainContents.ECS
{
    public sealed class PlayerMoveSystem : ComponentSystem
    {
        struct Group
        {
            public readonly int Length;
            public ComponentDataArray<Position> Position;
            public ComponentDataArray<PlayerInput> Input;
            [ReadOnly] public SharedComponentDataArray<PlayerSettings> PlayerSettings;
        }

        [Inject] Group _group;

        protected override void OnUpdate()
        {
            float deltaTime = Time.deltaTime;

            for (int i = 0; i < this._group.Length; ++i)
            {
                var playerSettings = this._group.PlayerSettings[i];
                var pos = this._group.Position[i].Value;
                var input = this._group.Input[i];

                var movable = playerSettings.MovableAreaInstance;
                pos = new float3(MainECS_Manager.WorldMousePosision.x, 0, MainECS_Manager.WorldMousePosision.z);

                if (pos.x <= movable.xMin)
                {
                    pos.x = movable.xMin;
                }
                else if (pos.x > movable.xMax)
                {
                    pos.x = movable.xMax;
                }

                if (pos.y <= movable.yMin)
                {
                    pos.y = movable.yMin;
                }
                else if (pos.y > movable.yMax)
                {
                    pos.y = movable.yMax;
                }


                var retPos = new Position { Value = pos };
                if (input.Fire == 1)
                {
                    input.FireCooldown = playerSettings.ShootSettingsInstance.FireCooldown;

                    PostUpdateCommands.CreateEntity(MainECS_Manager.PlayerBulletArchetype);
                    PostUpdateCommands.SetComponent(retPos);
                    PostUpdateCommands.SetComponent(
                        new BulletData
                        {
                            ShotSpeed = playerSettings.ShootSettingsInstance.ShotSpeed,
                            // 上方向固定で発射
                            ShotAngle = 90,
                            Lifespan = playerSettings.ShootSettingsInstance.Lifespan
                        });
                    PostUpdateCommands.SetSharedComponent(MainECS_Manager.PlayerBulletLook);
                    PostUpdateCommands.SetSharedComponent(MainECS_Manager.BulletCollision);
                }

                this._group.Position[i] = retPos;
                this._group.Input[i] = input;
            }
        }
    }
}