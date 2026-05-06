using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace HenryLab
{
    /// <summary>
    /// Player Move Action
    /// </summary>
    public class MoveAction : BaseAction
    {
        private const float DestinationSampleRadius = 1.0f;

        private NavMeshAgent _agent;
        private float _speed;
        private Vector3 _destination;

        public MoveAction(NavMeshAgent agent, float speed, Vector3 destination)
        {
            Name = "MoveAction";
            _agent = agent;
            _speed = speed;
            _destination = destination;
        }

        public override async Task Execute()
        {
            await base.Execute();

            if(_agent == null)
            {
                LogSpatialReachabilityFailure("agent_missing", _destination, _destination);
                return;
            }

            if(!_agent.isActiveAndEnabled)
            {
                LogSpatialReachabilityFailure("agent_inactive", _destination, _destination);
                return;
            }

            if(!_agent.isOnNavMesh)
            {
                LogSpatialReachabilityFailure("agent_off_navmesh", _destination, _destination);
                return;
            }

            if(!NavMesh.SamplePosition(_destination, out NavMeshHit targetHit, DestinationSampleRadius, NavMesh.AllAreas))
            {
                LogSpatialReachabilityFailure("target_off_navmesh", _destination, _destination);
                return;
            }

            Vector3 navMeshDestination = targetHit.position;
            NavMeshPath path = new NavMeshPath();
            if(!NavMesh.CalculatePath(_agent.transform.position, navMeshDestination, NavMesh.AllAreas, path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                LogSpatialReachabilityFailure("blocked_path", _destination, navMeshDestination, path.status.ToString());
                return;
            }

            if(!_agent.SetDestination(navMeshDestination))
            {
                LogSpatialReachabilityFailure("set_destination_failed", _destination, navMeshDestination, path.status.ToString());
                return;
            }

            _agent.speed = _speed;
            while(_agent.pathPending)
                await Task.Yield();

            float startTime = Time.time;
            const float maxWaitTime = 30f;

            while(_agent && _agent.isActiveAndEnabled && _agent.isOnNavMesh &&
                   (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance))
            {
                if(Time.time - startTime > maxWaitTime)
                {
                    MarkFailed("spatial_reachability:movement_timeout");
                    Debug.LogError($"{Str.Tags.LogsTag} [XRGate] failure_type=spatial_reachability condition_kind=spatial_reachability reason=movement_timeout requested_destination={_destination} navmesh_destination={navMeshDestination}");
                    break;
                }
                await Task.Yield();
            }
        }

        private void LogSpatialReachabilityFailure(string reason, Vector3 requestedDestination, Vector3 navMeshDestination, string pathStatus = "")
        {
            MarkFailed($"spatial_reachability:{reason}");
            Debug.LogWarning($"{Str.Tags.LogsTag} [XRGate] failure_type=spatial_reachability condition_kind=spatial_reachability reason={reason} requested_destination={requestedDestination} navmesh_destination={navMeshDestination} path_status={pathStatus}");
        }
    }
}