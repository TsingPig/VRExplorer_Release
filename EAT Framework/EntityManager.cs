using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HenryLab
{
    public class EntityManager : Singleton<EntityManager>
    {
        /// <summary>
        /// VRExplorer��Monoָ��
        /// </summary>
        public MonoBehaviour vrexplorerMono;

        /// <summary>
        /// �洢ÿ��Entity�Ĵ���״̬
        /// </summary>
        public Dictionary<IBaseEntity, HashSet<Enum>> entityStates = new Dictionary<IBaseEntity, HashSet<Enum>>();

        /// <summary>
        /// �洢 mono��Ӧ������Entityӳ��
        /// </summary>
        public Dictionary<MonoBehaviour, List<IBaseEntity>> monoEntitiesMapping = new Dictionary<MonoBehaviour, List<IBaseEntity>>();

        /// <summary>
        /// �洢����mono�Ƿ�̽����
        /// </summary>
        public Dictionary<MonoBehaviour, bool> monoState = new Dictionary<MonoBehaviour, bool>();

        /// <summary>
        /// ����mono״̬�����ж��Ƿ���Ҫ��������
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool UpdateMonoState(MonoBehaviour mono, bool value)
        {
            if(monoState.ContainsKey(mono))
            {
                monoState[mono] = value;
                if(value && monoState.Values.All(value => value))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// ע������ʵ�壬��Ҫ��ʼ��ʱ����
        /// </summary>
        public void RegisterAllEntities()
        {
            var entityTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(IBaseEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach(var entityType in entityTypes)
            {
                var allEntities = FindObjectsOfType(entityType);
                foreach(var entity in allEntities)
                {
                    RegisterEntity((IBaseEntity)entity);
                }
            }
        }

        /// <summary>
        /// ע��ʵ�岢��ʼ��״̬
        /// </summary>
        /// <param name="entity"></param>
        public void RegisterEntity(IBaseEntity entity)
        {
            MonoBehaviour mono = entity.transform.GetComponent<MonoBehaviour>();

            if(!monoEntitiesMapping.ContainsKey(mono))
            {
                monoEntitiesMapping[mono] = new List<IBaseEntity>();
            }
            monoEntitiesMapping[mono].Add(entity);
            monoState[mono] = false;

            if(!entityStates.ContainsKey(entity))
            {
                Debug.Log($"{Str.Tags.LogsTag}Entity'{entity.Name}' Registered");
                entityStates[entity] = new HashSet<Enum>();

                var interfaces = entity.GetType().GetInterfaces();
                foreach(var iface in interfaces)
                {
                    var nestedTypes = iface.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach(var nestedType in nestedTypes)
                    {
                        if(nestedType.IsEnum)
                        {
                            var enumValues = Enum.GetValues(nestedType);
                            ExperimentManager.Instance.StateCount += enumValues.Length;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ����ʵ��״̬
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="state"></param>
        public void TriggerState(IBaseEntity entity, Enum state)
        {
            if(entityStates.ContainsKey(entity) && !entityStates[entity].Contains(state))
            {
                entityStates[entity].Add(state);
                Debug.Log(new RichText()
                    .Add($"Entity ", bold: true)
                    .Add(entity.Name, bold: true, color: new Color(1.0f, 0.64f, 0.0f))
                    .Add(" State ", bold: true)
                    .Add(state.ToString(), bold: true, color: Color.green)
                    .GetText());
            }
        }

        public T GetEntity<T>(MonoBehaviour mono) where T : class, IBaseEntity
        {
            if(monoEntitiesMapping.TryGetValue(mono, out var entities))
            {
                foreach(var entity in entities)
                {
                    if(entity is T targetEntity)
                    {
                        return targetEntity;
                    }
                }
            }
            Debug.LogError($"{mono} GetEntity result is null");
            return null;
        }

        /// <summary>
        /// ����ʵ������
        /// </summary>
        public void ResetAllEntites()
        {
            // �������ʵ���״̬����
            foreach(var entity in entityStates.Keys.ToList())
            {
                entityStates[entity].Clear();
            }

            // ������mono��̽��״̬��Ϊfalse
            foreach(var mono in monoState.Keys.ToList())
            {
                monoState[mono] = false;
            }

            Debug.Log($"{Str.Tags.LogsTag}All Entities Reset");
        }
    }
}