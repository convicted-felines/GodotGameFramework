//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 流程组件。管理游戏流程（Procedure），驱动游戏生命周期状态机。
    /// 在 Inspector 中配置流程类型名称和入口流程类型名称。
    /// </summary>
    public sealed partial class ProcedureComponent : GameFrameworkComponent
    {
        [Godot.Export] private string[] m_AvailableProcedureTypeNames = Array.Empty<string>();
        [Godot.Export] private string m_EntranceProcedureTypeName = string.Empty;

        private IProcedureManager m_ProcedureManager = null;

        /// <summary>当前流程。</summary>
        public ProcedureBase CurrentProcedure => m_ProcedureManager?.CurrentProcedure;

        /// <summary>当前流程持续时间。</summary>
        public float CurrentProcedureTime => m_ProcedureManager?.CurrentProcedureTime ?? 0f;

        public override void _Ready()
        {
            base._Ready();

            m_ProcedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            if (m_ProcedureManager == null)
            {
                GameFrameworkLog.Fatal("Procedure manager is invalid.");
                return;
            }
        }

        /// <summary>
        /// 在 BaseComponent._Ready 之后调用（通常在场景 _Ready 末尾），
        /// 收集所有流程类型并启动入口流程。
        /// </summary>
        public void StartProcedures()
        {
            IFsmManager fsmManager = GameFrameworkEntry.GetModule<IFsmManager>();
            if (fsmManager == null)
            {
                GameFrameworkLog.Fatal("FSM manager is invalid.");
                return;
            }

            var procedures = new List<ProcedureBase>();
            foreach (string typeName in m_AvailableProcedureTypeNames)
            {
                Type procedureType = global::GameFramework.Utility.Assembly.GetType(typeName);
                if (procedureType == null)
                {
                    GameFrameworkLog.Error("Can not find procedure type '{0}'.", typeName);
                    return;
                }

                ProcedureBase procedure = Activator.CreateInstance(procedureType) as ProcedureBase;
                if (procedure == null)
                {
                    GameFrameworkLog.Error("Can not create procedure instance '{0}'.", typeName);
                    return;
                }

                procedures.Add(procedure);
            }

            m_ProcedureManager.Initialize(fsmManager, procedures.ToArray());

            if (string.IsNullOrEmpty(m_EntranceProcedureTypeName))
            {
                GameFrameworkLog.Error("Entrance procedure type name is invalid.");
                return;
            }

            Type entranceType = global::GameFramework.Utility.Assembly.GetType(m_EntranceProcedureTypeName);
            if (entranceType == null)
            {
                GameFrameworkLog.Error("Can not find entrance procedure type '{0}'.", m_EntranceProcedureTypeName);
                return;
            }

            m_ProcedureManager.StartProcedure(entranceType);
        }

        public bool HasProcedure<T>() where T : ProcedureBase => m_ProcedureManager.HasProcedure<T>();

        public bool HasProcedure(Type procedureType) => m_ProcedureManager.HasProcedure(procedureType);

        public ProcedureBase GetProcedure<T>() where T : ProcedureBase => m_ProcedureManager.GetProcedure<T>();

        public ProcedureBase GetProcedure(Type procedureType) => m_ProcedureManager.GetProcedure(procedureType);
    }
}
