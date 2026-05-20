using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ForTheCompany.Core;
using ForTheCompany.Player;

namespace ForTheCompany.Systems
{
    public class SecurityQuizController : MonoBehaviour
    {
        public static SecurityQuizController Instance { get; private set; }

        public ClueObject ActiveClue { get; private set; }
        public bool IsOpen => ActiveClue != null;
        public string LastResultText { get; private set; }
        public float LastResultTime { get; private set; }
        public bool LastWasCorrect { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureSpawned()
        {
            if (FindFirstObjectByType<SecurityQuizController>() != null) return;
            var go = new GameObject("SecurityQuizController");
            var ctrl = go.AddComponent<SecurityQuizController>();
            ctrl.SpawnDefaultClues();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Update()
        {
            if (!IsOpen) return;
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) Close();
        }

        public void Open(ClueObject clue)
        {
            if (clue == null || clue.Resolved) return;
            ActiveClue = clue;
        }

        public void Close() => ActiveClue = null;

        public void Answer(int index)
        {
            if (ActiveClue == null || ActiveClue.data == null) return;
            var data = ActiveClue.data;
            bool correct = index == data.correctIndex;
            LastWasCorrect = correct;
            LastResultTime = Time.time;

            if (correct)
            {
                if (GameSession.Instance != null)
                {
                    GameSession.Instance.totalClues += data.clueReward;
                    GameSession.Instance.LastEncounterRewardClues = data.clueReward;
                }
                LastResultText = $"✓ 정답!\n{data.successClue}\n+{data.clueReward} 단서 획득";
                ActiveClue.MarkResolved();
                Debug.Log($"[Quiz] {data.id} 정답 — '{data.successClue}' (+{data.clueReward} 단서)");
                ActiveClue = null;
            }
            else
            {
                LastResultText = "✗ 오답. 보안교육 자료 학습 후 다시 시도.";
                Debug.Log($"[Quiz] {data.id} 오답");
            }
        }

        private void SpawnDefaultClues()
        {
            var allClues = GetDefaultClues();
            foreach (var data in allClues)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Clue_" + data.id;
                go.transform.position = data.worldPos;
                go.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

                // Remove collider so player can walk through
                var bc = go.GetComponent<BoxCollider>();
                if (bc != null) Destroy(bc);

                var mr = go.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    var mpb = new MaterialPropertyBlock();
                    mpb.SetColor("_BaseColor", data.color);
                    mr.SetPropertyBlock(mpb);
                }

                var co = go.AddComponent<ClueObject>();
                co.data = data;
            }
        }

        public static List<ClueData> GetDefaultClues()
        {
            return new List<ClueData>
            {
                new ClueData
                {
                    id = "research_usb",
                    roomName = "연구실",
                    worldPos = new Vector3(-20f, 0.6f, 14f),
                    objectLabel = "정체불명 USB",
                    color = new Color(0.95f, 0.85f, 0.3f),
                    promptVerb = "조사",
                    quizQuestion = "신원 미상의 USB가 발견됐다. 가장 안전한 대응은?",
                    quizOptions = new[]
                    {
                        "내 컴퓨터에 꽂아 내용을 확인한다",
                        "신고 후 보안팀에 전달한다",
                        "그냥 무시한다",
                        "다른 직원에게 준다"
                    },
                    correctIndex = 1,
                    successClue = "USB에 새벽 2시 연구실 서버 백업 데이터가 들어있다. 누군가 빼돌리려 했다.",
                    clueReward = 2
                },
                new ClueData
                {
                    id = "server_log",
                    roomName = "서버실",
                    worldPos = new Vector3(3f, 0.6f, 14f),
                    objectLabel = "서버실 모니터",
                    color = new Color(0.95f, 0.3f, 0.3f),
                    promptVerb = "로그 확인",
                    quizQuestion = "관리자 권한으로 접근한 비정상 로그를 발견했다. 우선 조치는?",
                    quizOptions = new[]
                    {
                        "무시하고 일을 계속한다",
                        "로그를 삭제해서 정리한다",
                        "로그 보존 후 보안팀에 즉시 보고",
                        "본인이 해당 사용자에게 직접 묻는다"
                    },
                    correctIndex = 2,
                    successClue = "비밀번호 평문 저장 + 비정상 관리자 접근. 내부자 소행 가능성이 높다.",
                    clueReward = 2
                },
                new ClueData
                {
                    id = "data_traffic",
                    roomName = "데이터센터",
                    worldPos = new Vector3(3f, 0.6f, -14f),
                    objectLabel = "트래픽 분석 단말",
                    color = new Color(0.3f, 0.7f, 0.85f),
                    promptVerb = "분석",
                    quizQuestion = "외부 IP로 대용량 데이터 전송이 감지되었다. 적절한 대응은?",
                    quizOptions = new[]
                    {
                        "전송 완료까지 기다린다",
                        "전송을 즉시 차단하고 로그 분석",
                        "본인이 받은 게 아니니 무시",
                        "동료에게 알리고 퇴근"
                    },
                    correctIndex = 1,
                    successClue = "어제 23:47, 미식별 외부 IP로 1.2GB 전송. 발신 단말은 사내 네트워크 단말.",
                    clueReward = 2
                },
                new ClueData
                {
                    id = "lounge_memo",
                    roomName = "휴게실",
                    worldPos = new Vector3(-15f, 0.6f, -2f),
                    objectLabel = "휘갈긴 메모",
                    color = new Color(0.95f, 0.95f, 0.95f),
                    promptVerb = "읽기",
                    quizQuestion = "쓰레기통에서 비밀번호가 적힌 메모를 발견했다. 처리 방법?",
                    quizOptions = new[]
                    {
                        "적힌 비밀번호로 시스템 접근 시도",
                        "분쇄기로 즉시 파기 + 정책 안내",
                        "SNS에 사진을 올린다",
                        "그냥 다시 버린다"
                    },
                    correctIndex = 1,
                    successClue = "메모에 'adm1n_2026' 같은 관리자 패스워드. 누군가 메모를 보고 무단 접속했다.",
                    clueReward = 2
                },
                new ClueData
                {
                    id = "storage_box",
                    roomName = "창고",
                    worldPos = new Vector3(-20f, 0.6f, -14f),
                    objectLabel = "수상한 택배",
                    color = new Color(0.65f, 0.45f, 0.25f),
                    promptVerb = "조사",
                    quizQuestion = "발신자 불명의 익명 택배가 도착했다. 적절한 행동?",
                    quizOptions = new[]
                    {
                        "즉시 개봉한다",
                        "보안팀 신고 + X-ray 검사",
                        "책상 위에 그냥 둔다",
                        "본인이 가져간다"
                    },
                    correctIndex = 1,
                    successClue = "위장 USB 충전기 + 키로거가 들어있다. 명백한 사회공학 공격 시도.",
                    clueReward = 2
                },
                new ClueData
                {
                    id = "cardkey_log",
                    roomName = "카드키 구역",
                    worldPos = new Vector3(18f, 0.6f, 2f),
                    objectLabel = "카드키 발급 로그",
                    color = new Color(0.85f, 0.2f, 0.2f),
                    promptVerb = "조회",
                    quizQuestion = "내 카드키가 사용된 적 없는 시간대 출입 기록이 발견됐다. 우선 조치?",
                    quizOptions = new[]
                    {
                        "사용 가능성을 무시한다",
                        "카드 분실/도난 가능성 신고 + 재발급",
                        "비밀번호를 바꾼다 (카드와 무관)",
                        "친구에게 빌려준 것이라 추측"
                    },
                    correctIndex = 1,
                    successClue = "복제 카드키가 새벽 1~3시 보안 구역 출입에 사용됐다.",
                    clueReward = 2
                }
            };
        }
    }
}
