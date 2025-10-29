using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HenryLab
{
    public class ExperimentManager : Singleton<ExperimentManager>
    {
        public float reportCoverageDuration = 15f;

        public event Action ExperimentFinishEvent;

        private float _timeStamp;

        private StringBuilder _csvDataBuilder = new StringBuilder();

        /// <summary>
        /// ��ȡ�ܴ���״̬����
        /// </summary>
        public int TriggeredStateCount
        {
            get
            {
                int res = 0;
                foreach(var v in EntityManager.Instance.entityStates.Values)
                {
                    res += v.Count;
                }
                return res;
            }
        }

        /// <summary>
        /// ��ȡ��״̬����
        /// </summary>
        public int StateCount { get; set; }

        /// <summary>
        /// ��ȡ�ܿɽ����������
        /// </summary>
        public int InteractableCount
        {
            get { return EntityManager.Instance.monoState.Count; }
        }

        /// <summary>
        /// ��ȡ��̽�����Ŀɽ����������
        /// </summary>
        public int CoveredInteractableCount
        {
            get { return EntityManager.Instance.monoState.Count((monoPair) => { return monoPair.Value == true; }); }
        }

        public void ShowMetrics()
        {
            var metricInfo = new RichText()
                .Add("TimeCost: ").Add((Time.time - _timeStamp).ToString(), bold: true, color: Color.yellow)
                .Add(", CoveredInteractableCount: ", bold: true).Add(CoveredInteractableCount.ToString(), bold: true, color: Color.yellow)
                .Add(", InteractableCount: ", bold: true).Add(InteractableCount.ToString(), bold: true, color: Color.yellow)
                .Add(", Interactable Coverage: ", bold: true).Add($"{CoveredInteractableCount * 100f / InteractableCount:F2}%", bold: true, color: Color.yellow);

            Debug.Log(metricInfo);

            _csvDataBuilder.AppendLine($"{Time.time - _timeStamp},{TriggeredStateCount},{StateCount},{CoveredInteractableCount},{InteractableCount},{CoveredInteractableCount * 100f / InteractableCount:F2},{TriggeredStateCount * 100f / StateCount:F2}");
            // CodeCoverage.GenerateReportWithoutStopping();  Had Removed Code Coverage(revised version) from Framwork
        }

        private void OnApplicationQuit()
        {
            ExperimentFinish();
            SaveMetricsToCSV();
            //CodeCoverage.StopRecording();
        }

        public void ExperimentFinish()
        {
            ShowMetrics();
            Debug.Log(new RichText().Add("Experiment Finished", color: Color.yellow, bold: true));
            //StateCount = 0;
            ExperimentFinishEvent?.Invoke();
            StopAllCoroutines();
            //UnityEditor.EditorApplication.isPlaying = false;
        }

        public void StartRecording()
        {
            _timeStamp = Time.time;
            StartCoroutine("RecordCoroutine");
            ShowMetrics();
        }

        private IEnumerator RecordCoroutine()
        {
            float lastTime = Time.time;
            while(true)
            {
                if(Time.time - lastTime >= reportCoverageDuration)
                {
                    ShowMetrics();
                    lastTime = Time.time;
                }
                yield return null;
            }
        }

        /// <summary>
        /// ����ָ�����ݵ�CSV�ļ�
        /// </summary>
        private void SaveMetricsToCSV()
        {
            string filePath = Path.Combine(Application.dataPath, "../_Experiment/InteractableAndStateCoverageReport.csv");

            string directoryPath = Path.GetDirectoryName(filePath);
            if(!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if(!File.Exists(filePath))
            {
                _csvDataBuilder.Insert(0, "TimeCost,TriggeredStateCount,StateCount,CoveredInteractableCount,InteractableCount,InteractableCoverage,StateCountCoverage\n");
            }

            File.WriteAllText(filePath, _csvDataBuilder.ToString());
            Debug.Log($"Metrics saved to {filePath}");
        }
    }
}