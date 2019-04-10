using System.Collections.Generic;
using System.Text;
using NukeGuard_TractorBeam.TractorBeams.Utilities;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace NukeGuard_TractorBeam.TractorBeams
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_LargeGatlingTurret), true, "LargeTractorBeam", "TractorBeam")]
    public class TractorBeamTurret : MyGameLogicComponent
    {
	    readonly MyDefinitionId _electricityDefinition = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");

        MyObjectBuilder_EntityBase _objectBuilder = null;

        MyEntity3DSoundEmitter _e;


        IMyCubeBlock _cubeBlock = null;
        Sandbox.ModAPI.IMyFunctionalBlock _functionalBlock = null;
        Sandbox.ModAPI.IMyTerminalBlock _terminalBlock;

        MyResourceSinkComponent _resourceSink;
        IMyInventory _mInventory;

        string _subtypeName;

        AttractorWeaponInfo _attractorWeaponInfo;

        float _powerConsumption;
        float _setPowerConsumption;

        float _currentHeat;
        readonly bool _overheated = false;

        long _lastShootTime;
        int _lastShootTimeTicks;


        bool _hitBool = false;

        int _ticks = 0;

        readonly int _damageUpgrades = 0;
        float _heatUpgrades = 0;
        float _efficiencyUpgrades = 1f;

        List<MyObjectBuilder_AmmoMagazine> _chargeObjectBuilders;
        List<SerializableDefinitionId> _chargeDefinitionIds;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            _objectBuilder = objectBuilder;

            Entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;

            _functionalBlock = Entity as Sandbox.ModAPI.IMyFunctionalBlock;
            _cubeBlock = Entity as IMyCubeBlock;
            _terminalBlock = Entity as Sandbox.ModAPI.IMyTerminalBlock;

            _subtypeName = _functionalBlock.BlockDefinition.SubtypeName;

            GetAttractorWeaponInfo(_subtypeName);
            InitCharges();

            _cubeBlock.AddUpgradeValue("PowerEfficiency", 1.0f);
            _cubeBlock.OnUpgradeValuesChanged += OnUpgradeValuesChanged;

            _terminalBlock.AppendingCustomInfo += AppendCustomInfo;

            IMyCubeBlock cube = Entity as IMyCubeBlock;
            _lastShootTime = ((MyObjectBuilder_LargeGatlingTurret)cube.GetObjectBuilderCubeBlock()).GunBase.LastShootTime;

        }

        public override void UpdateBeforeSimulation100()
        {
            if (Ui == null)
            {
                Ui = new TractorUi<Sandbox.ModAPI.Ingame.IMyLargeTurretBase>();
                Ui.CreateUi((Sandbox.ModAPI.IMyTerminalBlock)Entity);
            }
        }

        public TractorUi<Sandbox.ModAPI.Ingame.IMyLargeTurretBase> Ui;

        private void OnUpgradeValuesChanged() {

            if (Entity != null) {

                _efficiencyUpgrades = _cubeBlock.UpgradeValues["PowerEfficiency"];

            }
        }

        public void AppendCustomInfo(Sandbox.ModAPI.IMyTerminalBlock block, StringBuilder info)
        {
            info.Clear();


            info.AppendLine("Type: " + _cubeBlock.DefinitionDisplayNameText);
            info.AppendLine("Required Input: " + _powerConsumption.ToString("N") + "MW");
            info.AppendLine("Maximum Input: " + _attractorWeaponInfo.PowerUsage.ToString("N") + "MW");

            info.AppendLine(" ");

            if (_attractorWeaponInfo.Classes > 1) {

                info.AppendLine("Class: " + "Class " + (_damageUpgrades + 1) + " Beam Weapon");

            }

            info.AppendLine("Heat: " + _currentHeat + "/" + (_attractorWeaponInfo.MaxHeat).ToString("N") + "C");
            info.AppendLine("Overheated: " + _overheated);
        }

        private void InitCharges() {

            _chargeObjectBuilders = new List<MyObjectBuilder_AmmoMagazine>();

            if (_attractorWeaponInfo.Classes == 1) {

                _chargeObjectBuilders.Add(new MyObjectBuilder_AmmoMagazine() { SubtypeName = "" + _attractorWeaponInfo.AmmoName });

            } else {

                for (int i = 1; i <= _attractorWeaponInfo.Classes; i++) {

                    _chargeObjectBuilders.Add(new MyObjectBuilder_AmmoMagazine() { SubtypeName = "" + "Class" + i + _attractorWeaponInfo.AmmoName });
                }
            }

            _chargeDefinitionIds = new List<SerializableDefinitionId>();

            if (_attractorWeaponInfo.Classes == 1) {

                _chargeDefinitionIds.Add(new SerializableDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), "" + _attractorWeaponInfo.AmmoName));

            } else {

                for (int i = 1; i <= _attractorWeaponInfo.Classes; i++) {

                    _chargeDefinitionIds.Add(new SerializableDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), "Class" + i + _attractorWeaponInfo.AmmoName));
                }
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return _objectBuilder;
        }

        private void GetAttractorWeaponInfo(string name) {

            if (_subtypeName == "LargeTractorBeam") {
                _attractorWeaponInfo = TractorBeamManager.LargeBlockAttractorTurret;
            }
			else if (_subtypeName == "TractorBeam") {
                _attractorWeaponInfo = TractorBeamManager.LargeBlockAttractorTurret;
            }
        }

        public override void UpdateOnceBeforeFrame()
        {

            _resourceSink = Entity.Components.Get<MyResourceSinkComponent>();

            _resourceSink.SetRequiredInputByType(_electricityDefinition, 0.0021f);
            _setPowerConsumption = 0.0081f;

            _mInventory = ((Sandbox.ModAPI.Ingame.IMyTerminalBlock)Entity).GetInventory(0) as IMyInventory;

        }

        public IMyCubeGrid GetTarget()
        {
            //var turretBase = Entity as Sandbox.ModAPI.IMyLargeTurretBase;
            //var fixedWeapon = Entity as Sandbox.ModAPI.IMyUserControllableGun;

            //if (turretBase != null)
            //{
            //    target = turretBase.Target;
            //}

            try {
                MyEntitySubpart subpart1 = _cubeBlock.GetSubpart("GatlingTurretBase1");
                MyEntitySubpart subpart2 = subpart1.GetSubpart("GatlingTurretBase2");

                if (_cubeBlock == null || _cubeBlock.CubeGrid == null || subpart1 == null || subpart2 == null || subpart1.WorldMatrix == null || subpart2.WorldMatrix == null) { return null; }

                Vector3D from = subpart2.WorldMatrix.Translation + subpart2.WorldMatrix.Forward * 0.3d;
                Vector3D to = subpart2.WorldMatrix.Translation + subpart2.WorldMatrix.Forward * 800d;

                LineD ray = new LineD(from, to);
                List<MyLineSegmentOverlapResult<MyEntity>> result = new List<MyLineSegmentOverlapResult<MyEntity>>();
                MyGamePruningStructure.GetTopmostEntitiesOverlappingRay(ref ray, result, MyEntityQueryType.Both);

                foreach (MyLineSegmentOverlapResult<MyEntity> resultItem in result)
                {
                    if (resultItem.Element == null) { continue; }

                    if (resultItem.Element.EntityId != _cubeBlock.CubeGrid.EntityId)
                    {
                        if (resultItem.Element is IMyCubeGrid)
                        {
                            return resultItem.Element as IMyCubeGrid;
                        }
                    }
                }
            }
            catch (KeyNotFoundException)
            {

            }
            return null;

        }

        public override void UpdateBeforeSimulation()
        {
            if (Ui == null || !Ui.Initialized) { return; }
            if (Entity == null) { return; }

            IMyCubeBlock cube = Entity as IMyCubeBlock;
            IMyCubeGrid target = GetTarget();
            

            bool isShooting = (Entity as Sandbox.ModAPI.IMyUserControllableGun).IsShooting;
            if (isShooting && target != null && target.Physics != null)
            {
                IMyCubeGrid grid = target;
                MyEntitySubpart subpart1 = _cubeBlock.GetSubpart("GatlingTurretBase1");
                MyEntitySubpart subpart2 = subpart1.GetSubpart("GatlingTurretBase2");

                if (subpart1 == null || subpart2 == null || subpart1.WorldMatrix == null || subpart2.WorldMatrix == null) { return; }

                Vector3D from = subpart2.WorldMatrix.Translation + subpart2.WorldMatrix.Forward * 0.3d;
                Vector3D to = target.Physics.CenterOfMassWorld;
                Vector3D toTarget = to - from;
                toTarget.Normalize();


                double distance = Vector3D.Distance(from, to);
                float min = Ui.MinSlider.Getter(_terminalBlock);
                float max = Ui.MaxSlider.Getter(_terminalBlock);
                float force = Ui.StrengthSlider.Getter(_terminalBlock);
                Vector3D forceVector = force * toTarget;

                if (distance > max)
                {
                    grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, -forceVector, null, null);
                }
                else if (distance < min)
                {
                    double percentage = 1 - (distance * distance) / (min * min);
                    Vector3D additionalForce = toTarget * percentage * (Ui.StrengthSlider.Max - force);
                    Vector3D velocity = new Vector3D(grid.Physics.LinearVelocity);
					velocity.Normalize();
                    grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, forceVector + additionalForce / 4, null, null);
                }
                else
                {
                    Vector3D velocity = new Vector3D(grid.Physics.LinearVelocity);
                    velocity.Normalize();
                    grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, force * -velocity / 3, null, null);
                }

                DrawShootingEffect(from, to);
            }
            Recharge();
        }

        public void DrawShootingEffect(Vector3D from, Vector3D to)
        {
            Vector4 maincolor = Color.White.ToVector4();
            Vector4 auxcolor = Color.Blue.ToVector4();
            MyStringId material = MyStringId.GetOrCompute("WeaponLaser");

            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                if (!MyAPIGateway.Session.CreativeMode)
                {
                    MySimpleObjectDraw.DrawLine(from, to, material, ref auxcolor, 0.15f * (_currentHeat / _attractorWeaponInfo.MaxHeat + 0.2f));
                    MySimpleObjectDraw.DrawLine(from, to, material, ref maincolor, 0.5f * (_currentHeat / _attractorWeaponInfo.MaxHeat + 0.2f));
                }
                else
                {
                    MySimpleObjectDraw.DrawLine(from, to, material, ref auxcolor, 0.15f * 1.2f);
                    MySimpleObjectDraw.DrawLine(from, to, material, ref maincolor, 0.5f * 1.2f);
                }
            }
        }

        void Recharge()
        {
            int chargesInInventory = (int)_mInventory.GetItemAmount(_chargeDefinitionIds[_damageUpgrades]);
            if (chargesInInventory < _attractorWeaponInfo.KeepAtCharge) {

				if (_resourceSink.RequiredInputByType(_electricityDefinition) != (_attractorWeaponInfo.PowerUsage/_efficiencyUpgrades)) {
					
					_resourceSink.SetRequiredInputByType (_electricityDefinition, (_attractorWeaponInfo.PowerUsage/_efficiencyUpgrades));

					_setPowerConsumption = (_attractorWeaponInfo.PowerUsage/_efficiencyUpgrades);
					_powerConsumption = (_attractorWeaponInfo.PowerUsage/_efficiencyUpgrades);

				} else {

					if (!_functionalBlock.Enabled) {
						
						_powerConsumption = 0.0001f;
					}
				}

				if (_resourceSink.CurrentInputByType (_electricityDefinition) == (_attractorWeaponInfo.PowerUsage/_efficiencyUpgrades)) {

					if (!_overheated) {
						_mInventory.AddItems ((MyFixedPoint)(_attractorWeaponInfo.KeepAtCharge - chargesInInventory), _chargeObjectBuilders [_damageUpgrades]);
					}
				}

			} else if(chargesInInventory > _attractorWeaponInfo.KeepAtCharge) {
				
				_mInventory.RemoveItemsOfType ((MyFixedPoint)(chargesInInventory - _attractorWeaponInfo.KeepAtCharge), _chargeObjectBuilders [_damageUpgrades]);

			} else  {
				
				if (_setPowerConsumption != 0.0001f) {

					_resourceSink.SetRequiredInputByType (_electricityDefinition, 0.0001f);

					_setPowerConsumption = 0.0001f;
					_powerConsumption = 0.0001f;
				}
			}

            _terminalBlock.RefreshCustomInfo ();
		}

		public override void Close ()
		{

			if (_mInventory != null) {

				for (int i = 0; i < _attractorWeaponInfo.Classes; i++) { 
					_mInventory.RemoveItemsOfType (_mInventory.GetItemAmount (_chargeDefinitionIds[i]), _chargeObjectBuilders[i]);
				}
			}

			base.Close ();
		}

		public override void MarkForClose ()
		{
			if (_mInventory != null) {

				for (int i = 0; i < _attractorWeaponInfo.Classes; i++) { 
					_mInventory.RemoveItemsOfType (_mInventory.GetItemAmount (_chargeDefinitionIds[i]), _chargeObjectBuilders[i]);
				}
			}

			base.MarkForClose ();
		}

	}
}

