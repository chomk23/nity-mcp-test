using System.Collections.Generic;
using UnityEngine;
using ForTheCompany.Player;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// 보안 교육 문제 풀 — 방/카테고리별로 여러 문제 모아두고 매 모달 오픈 시 랜덤 1개 선택.
    /// 총 20문제 (연구실 7 / 서버실 7 / 시설 6).
    /// SecurityQuizController.Open(clue)에서 ApplyRandomTo(clue.data) 호출 → 풀에서 1개 골라 덮어쓰기.
    /// </summary>
    public static class QuizPool
    {
        public class QuizVariant
        {
            public string question;
            public string[] options;
            public int correctIndex;
            public string successClue;
            public int clueReward = 2;
        }

        private static Dictionary<string, List<QuizVariant>> pools;

        private static void EnsureInit()
        {
            if (pools != null) return;
            pools = new Dictionary<string, List<QuizVariant>>();

            // ═══════════ 연구실 USB / 외부 매체 / 자료 관리 (7개) ═══════════
            pools["research_usb"] = new List<QuizVariant>
            {
                new QuizVariant {
                    question = "신원 미상의 USB가 발견됐다. 가장 안전한 대응은?",
                    options = new[] {
                        "내 컴퓨터에 꽂아 내용을 확인한다",
                        "신고 후 보안팀에 전달한다",
                        "그냥 무시한다",
                        "다른 직원에게 준다"
                    },
                    correctIndex = 1,
                    successClue = "USB에 새벽 2시 연구실 서버 백업 데이터가 들어있다. 누군가 빼돌리려 했다.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "외부 협력사가 USB로 자료를 전달했다. 적절한 처리는?",
                    options = new[] {
                        "바로 본인 PC에 연결해 자료 확인",
                        "격리된 보안 단말에서 백신 검사 후 사내 공유 폴더로 이관",
                        "이메일로 다시 보내달라고 요청",
                        "분실 위험 있으니 USB를 사물함에 넣는다"
                    },
                    correctIndex = 1,
                    successClue = "협력사 USB에 평소와 다른 .exe 파일 다수. 사회공학 공격 시도로 추정.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "본인 개인 USB에 회사 자료를 잠깐 옮겨 집에서 작업하려 한다. 올바른가?",
                    options = new[] {
                        "마감 임박이니 그대로 진행",
                        "회사 정책상 금지 — 사내 보안 폴더 또는 승인된 클라우드만 사용",
                        "팀장에게 메신저로만 알리고 진행",
                        "암호화한 USB면 괜찮다"
                    },
                    correctIndex = 1,
                    successClue = "개인 USB 무단 반출 시도 흔적. 자료 외부 유출 가능성 시사.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "출장 중 카페 공공 와이파이로 회사 시스템에 접속하려 한다. 안전한 방법은?",
                    options = new[] {
                        "공공 와이파이는 무료니 그대로 접속",
                        "회사 VPN을 통해서만 접속",
                        "비밀번호만 강하게 설정하면 OK",
                        "휴대폰 데이터 테더링이면 무조건 안전"
                    },
                    correctIndex = 1,
                    successClue = "출장 직원 노트북에서 VPN 우회 흔적. 외부 망에서 사내 접근 시도.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "회사 자료를 출력했는데 회의 후 미사용 출력물이 남았다. 처리법?",
                    options = new[] {
                        "쓰레기통에 그냥 버린다",
                        "보안 분쇄기(파쇄기)에 폐기",
                        "책상 서랍에 넣어둔다",
                        "재활용 종이함에 넣는다"
                    },
                    correctIndex = 1,
                    successClue = "탕비실 쓰레기통에서 미파쇄 기밀 출력물 발견. 정보 유출 위험.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "잠깐 자리를 비울 때 컴퓨터 화면은?",
                    options = new[] {
                        "그대로 두고 다녀온다 (잠깐이니까)",
                        "Win+L (또는 Ctrl+Cmd+Q)로 화면 잠금",
                        "모니터 전원만 끈다",
                        "동료에게 봐달라고 한다"
                    },
                    correctIndex = 1,
                    successClue = "점심시간 직원 부재 중 PC 미잠금 다수. 내부 자료 무단 열람 정황.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "회사 PDF 자료를 개인 클라우드(드롭박스 등)에 백업하려 한다. 올바른가?",
                    options = new[] {
                        "백업은 좋은 습관이므로 OK",
                        "회사 정책상 금지 — 승인된 사내 백업 시스템만 사용",
                        "암호화한 zip이면 괜찮다",
                        "이메일로 본인에게 보내는 건 OK"
                    },
                    correctIndex = 1,
                    successClue = "직원 개인 드롭박스에 회사 기밀 PDF 다수. 정책 위반 + 외부 유출.",
                    clueReward = 2
                }
            };

            // ═══════════ 서버실 / 네트워크 / 권한 관리 (7개) ═══════════
            pools["server_log"] = new List<QuizVariant>
            {
                new QuizVariant {
                    question = "관리자 권한으로 접근한 비정상 로그를 발견했다. 우선 조치는?",
                    options = new[] {
                        "무시하고 일을 계속한다",
                        "로그를 삭제해서 정리한다",
                        "로그 보존 후 보안팀에 즉시 보고",
                        "본인이 해당 사용자에게 직접 묻는다"
                    },
                    correctIndex = 2,
                    successClue = "비밀번호 평문 저장 + 비정상 관리자 접근. 내부자 소행 가능성.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "외부 IP로 대용량 데이터 전송이 감지되었다. 적절한 대응은?",
                    options = new[] {
                        "전송 완료까지 기다린다",
                        "전송을 즉시 차단하고 로그 분석",
                        "본인이 받은 게 아니니 무시",
                        "동료에게 알리고 퇴근"
                    },
                    correctIndex = 1,
                    successClue = "어제 23:47, 미식별 외부 IP로 1.2GB 전송. 발신 단말은 사내 네트워크 단말.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "발신자가 회사 임원처럼 보이는 이메일에 .zip 첨부파일이 있다. 올바른 행동은?",
                    options = new[] {
                        "임원이니 빨리 열어본다",
                        "첨부파일 열기 전 발신자 주소를 다시 확인하고 의심되면 보안팀 신고",
                        "비밀번호를 회신으로 보낸다",
                        "동료들에게 전달해서 같이 보게 한다"
                    },
                    correctIndex = 1,
                    successClue = "임원 사칭 피싱 메일 다수. 발신 도메인이 미세하게 다름(o → 0).",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "여러 사이트에 같은 비밀번호를 쓰고 있다. 보안 측면에서?",
                    options = new[] {
                        "외우기 쉬워서 좋다",
                        "한 곳 유출되면 다른 사이트도 같이 뚫림 — 사이트별 다른 비밀번호 + 패스워드 매니저 사용",
                        "비밀번호가 길면 재사용해도 OK",
                        "회사 내부 시스템끼리만 같으면 괜찮다"
                    },
                    correctIndex = 1,
                    successClue = "직원 다수가 사내·사외 공통 비밀번호 사용. 외부 유출이 사내 침입으로 직결됨.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "회사 계정에 2단계 인증(2FA)이 설정 안 된 직원이 발견됐다. 위험성은?",
                    options = new[] {
                        "비밀번호만 있어도 충분하다",
                        "비밀번호 유출 시 즉시 침입 가능 — 모든 계정에 2FA 필수",
                        "관리자급만 2FA 필요하고 일반 직원은 불필요",
                        "외부 접근 안 하면 2FA 불필요"
                    },
                    correctIndex = 1,
                    successClue = "주요 계정 다수 2FA 미설정. 비밀번호 유출 시 즉시 내부망 침입 가능 상태.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "본인 계정에 새벽 3시 해외 IP 로그인 시도가 감지됐다. 어떻게?",
                    options = new[] {
                        "본인이 안 했으면 그냥 무시",
                        "즉시 비밀번호 변경 + 보안팀에 보고 + 세션 강제 종료",
                        "다음날 출근해서 천천히 처리",
                        "동료에게 자랑한다"
                    },
                    correctIndex = 1,
                    successClue = "직원 계정에 해외 IP 로그인 시도 다수. 자격증명 유출 정황.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "재택 중 VPN 없이 사내 시스템에 직접 접속하려 한다. 옳은가?",
                    options = new[] {
                        "공공 와이파이만 피하면 OK",
                        "절대 금지 — 반드시 회사 VPN 통과 후 접근",
                        "비밀번호가 강하면 VPN 없어도 무방",
                        "5분 안에 끝나면 괜찮다"
                    },
                    correctIndex = 1,
                    successClue = "재택 직원 일부 VPN 미경유 직접 접속 이력. 외부 가로채기 위험.",
                    clueReward = 2
                }
            };

            // ═══════════ 시설 / 출입 / 카드키 / 물리 보안 (6개) ═══════════
            pools["cardkey_log"] = new List<QuizVariant>
            {
                new QuizVariant {
                    question = "내 카드키가 사용된 적 없는 시간대 출입 기록이 발견됐다. 우선 조치?",
                    options = new[] {
                        "사용 가능성을 무시한다",
                        "카드 분실/도난 가능성 신고 + 즉시 재발급",
                        "비밀번호를 바꾼다 (카드와 무관)",
                        "친구에게 빌려준 것이라 추측"
                    },
                    correctIndex = 1,
                    successClue = "복제 카드키가 새벽 1~3시 보안 구역 출입에 사용됐다.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "보안 구역 CCTV가 평소보다 길게 점검 중으로 꺼져있다. 어떻게?",
                    options = new[] {
                        "점검 시간이니 그냥 둔다",
                        "관리자에게 즉시 확인 요청 + 그 시간대 출입 기록 대조",
                        "본인 PC로 다른 일을 한다",
                        "동료에게 잡담거리로 알린다"
                    },
                    correctIndex = 1,
                    successClue = "CCTV 점검 사유로 30분간 꺼졌으나 점검 일정 없음. 의도적 비활성 정황.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "낯선 외부인이 보안 구역 문 앞에서 사원증 없이 들어가려고 한다. 올바른 행동은?",
                    options = new[] {
                        "친절하게 문을 열어준다 (배달원 같으니)",
                        "정중히 거절하고 안내 데스크로 안내 + 보안팀에 보고",
                        "신원만 묻고 들여보낸다",
                        "본인 카드로 열어주고 자리 비운다"
                    },
                    correctIndex = 1,
                    successClue = "사회공학 무단 진입 시도 흔적. CCTV에 미사원 진입 다수 기록됨.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "회사 카드키를 분실했다. 즉시 해야 할 일은?",
                    options = new[] {
                        "내일까지 찾아보고 안 보이면 신고",
                        "즉시 시설관리팀에 신고 + 카드 무력화 + 재발급 요청",
                        "회사 책상 위에 메모만 남긴다",
                        "동료에게 부탁해서 본인 카드를 빌린다"
                    },
                    correctIndex = 1,
                    successClue = "직원 카드 분실 신고 지연 다수. 분실 카드가 외부 침입에 악용된 흔적.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "발신자 불명의 익명 택배가 본인 자리로 왔다. 적절한 행동?",
                    options = new[] {
                        "즉시 개봉한다",
                        "보안팀 신고 + X-ray 검사 또는 격리 보관",
                        "책상 위에 그냥 둔다",
                        "본인이 가져간다"
                    },
                    correctIndex = 1,
                    successClue = "위장 USB 충전기 + 키로거가 든 익명 택배. 명백한 사회공학 공격 시도.",
                    clueReward = 2
                },
                new QuizVariant {
                    question = "회의실에 기밀 문서와 노트북을 잠시 두고 화장실에 다녀왔다. 문제는?",
                    options = new[] {
                        "회의실 안이니 안전하다",
                        "단 1분도 자료·기기를 미감시 상태로 두면 안 됨 — 항상 휴대 또는 잠금 보관",
                        "동료가 있으면 괜찮다",
                        "문이 닫혀있으면 안전"
                    },
                    correctIndex = 1,
                    successClue = "회의실 미감시 자료에 외부인 접근 흔적. 사진 촬영 또는 USB 복사 가능성.",
                    clueReward = 2
                }
            };
        }

        /// <summary>
        /// 풀에서 중복 없이 N개 랜덤 선택 (Fisher-Yates 부분 셔플).
        /// 풀이 N보다 작으면 풀 전체 반환.
        /// SecurityQuizController가 한 세션(연속 3문제)에서 사용.
        /// </summary>
        public static List<QuizVariant> GetRandomBatch(string id, int count)
        {
            EnsureInit();
            if (!pools.TryGetValue(id, out var pool) || pool.Count == 0)
                return new List<QuizVariant>();

            int n = Mathf.Min(count, pool.Count);
            var indices = new List<int>(pool.Count);
            for (int i = 0; i < pool.Count; i++) indices.Add(i);

            var result = new List<QuizVariant>(n);
            for (int i = 0; i < n; i++)
            {
                int swap = Random.Range(i, indices.Count);
                (indices[i], indices[swap]) = (indices[swap], indices[i]);
                result.Add(pool[indices[i]]);
            }
            return result;
        }

        /// <summary>풀에서 랜덤 1개를 ClueData에 덮어쓰기 (단발 호환용).</summary>
        public static void ApplyRandomTo(ClueData data)
        {
            EnsureInit();
            if (data == null) return;
            if (!pools.TryGetValue(data.id, out var pool)) return;
            if (pool == null || pool.Count == 0) return;

            var picked = pool[Random.Range(0, pool.Count)];
            ApplyTo(data, picked);
        }

        /// <summary>주어진 QuizVariant를 ClueData의 quiz 필드에 적용.
        /// 옵션 순서는 Fisher-Yates 셔플 — 같은 문제도 매번 정답 위치가 다르게.</summary>
        public static void ApplyTo(ClueData data, QuizVariant q)
        {
            if (data == null || q == null) return;
            data.quizQuestion = q.question;

            // 옵션 셔플 + correctIndex 재계산
            int n = q.options.Length;
            var shuffled = new string[n];
            var origIdx = new int[n];
            for (int i = 0; i < n; i++) { shuffled[i] = q.options[i]; origIdx[i] = i; }
            for (int i = 0; i < n - 1; i++)
            {
                int swap = Random.Range(i, n);
                (shuffled[i], shuffled[swap]) = (shuffled[swap], shuffled[i]);
                (origIdx[i], origIdx[swap]) = (origIdx[swap], origIdx[i]);
            }
            int newCorrect = 0;
            for (int i = 0; i < n; i++)
                if (origIdx[i] == q.correctIndex) { newCorrect = i; break; }

            data.quizOptions = shuffled;
            data.correctIndex = newCorrect;
            data.successClue = q.successClue;
            data.clueReward = q.clueReward;
        }

        /// <summary>디버그용 — 풀 전체 개수 (NPC ID별)</summary>
        public static int GetPoolSize(string id)
        {
            EnsureInit();
            return pools.TryGetValue(id, out var pool) ? pool.Count : 0;
        }
    }
}
