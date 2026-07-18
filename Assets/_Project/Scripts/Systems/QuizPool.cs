using System.Collections.Generic;
using UnityEngine;
using ForTheCompany.Player;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// 보안 교육 문제 풀 — 방/카테고리별 고정 3문제 (총 9문항).
    /// 풀 크기 = 세션 문제 수(3)라 매 판 9문항이 전부 출제되고,
    /// 문제 순서·선택지 순서만 셔플된다 (SecurityQuizController가 GetRandomBatch 호출).
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

            // ═══════════ 연구실 — 외부 매체 / 자료 관리 (3개 고정) ═══════════
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
                    // 출처: 1교시 제6조 접근통제 - 외부 접속 시 안전한 접속·인증수단(VPN 등)
                    question = "출장 중 카페 공공 와이파이로 사내 시스템에 접속해야 한다. 안전한 방법은?",
                    options = new[] {
                        "공공 와이파이는 무료니 그대로 접속",
                        "가상사설망(VPN) 등 안전한 접속수단과 인증수단을 적용해 접속",
                        "비밀번호만 강하게 설정하면 OK",
                        "휴대폰 데이터 테더링이면 무조건 안전"
                    },
                    correctIndex = 1,
                    successClue = "출장 직원 노트북에서 VPN 우회 흔적. 외부 망에서 사내 접근 시도.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제6조 - 노출·공유설정 제한 / 3교시 - 검색엔진 노출 사고
                    question = "회사 기밀 PDF를 개인 클라우드(드롭박스 등)에 백업하려 한다. 무엇이 문제인가?",
                    options = new[] {
                        "백업은 좋은 습관이므로 OK",
                        "공유설정·외부 저장은 검색엔진 노출·유출로 이어질 수 있어 승인된 사내 시스템만 사용",
                        "암호화한 zip이면 괜찮다",
                        "이메일로 본인에게 보내는 건 OK"
                    },
                    correctIndex = 1,
                    successClue = "직원 개인 드롭박스에 회사 기밀 PDF 다수. 정책 위반 + 외부 유출.",
                    clueReward = 2
                }
            };

            // ═══════════ 서버실 — 네트워크 / 인증 (3개 고정) ═══════════
            pools["server_log"] = new List<QuizVariant>
            {
                new QuizVariant {
                    // 출처: 3교시 사고원인 - 임원 사칭 피싱메일(스피어피싱) / 발신 도메인 위조
                    question = "회사 임원을 사칭한 메일에 .zip 첨부와 '긴급 송금' 요청이 있다. 올바른 행동은?",
                    options = new[] {
                        "임원이니 빨리 열어본다",
                        "발신 주소(도메인 철자)를 다시 확인하고 의심되면 열지 말고 보안팀에 신고",
                        "비밀번호를 회신으로 보낸다",
                        "동료들에게 전달해서 같이 보게 한다"
                    },
                    correctIndex = 1,
                    successClue = "임원 사칭 피싱 메일 다수. 발신 도메인이 미세하게 다름(o → 0).",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 2교시 크리덴셜 스터핑 사례 - 유출된 ID/PW 무작위 대입 공격
                    question = "여러 사이트에 같은 비밀번호를 재사용하면 위험한 이유는? (크리덴셜 스터핑)",
                    options = new[] {
                        "외우기 쉬워서 오히려 좋다",
                        "한 곳에서 유출된 ID·PW를 다른 사이트에 무작위 대입해 뚫는 공격에 그대로 노출됨",
                        "비밀번호가 길면 재사용해도 OK",
                        "회사 내부 시스템끼리만 같으면 괜찮다"
                    },
                    correctIndex = 1,
                    successClue = "직원 다수가 사내·사외 공통 비밀번호 사용. 외부 유출이 사내 침입으로 직결됨.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 2교시 클라우드 2차인증 미적용 사례 / 1교시 제5조 인증수단(OTP)
                    question = "외부에서 개인정보처리시스템에 접속할 때 ID·비밀번호만 쓰고 있다. 무엇이 필요한가?",
                    options = new[] {
                        "비밀번호만 있어도 충분하다",
                        "OTP·인증서 등 2차 인증(안전한 인증수단)을 추가로 적용해야 한다",
                        "관리자급만 2차 인증하고 일반 직원은 불필요",
                        "내부 접속만 아니면 불필요"
                    },
                    correctIndex = 1,
                    successClue = "주요 계정 다수 2FA 미설정. 비밀번호 유출 시 즉시 내부망 침입 가능 상태.",
                    clueReward = 2
                }
            };

            // ═══════════ 시설 — 출입 / 물리 보안 (3개 고정) ═══════════
            pools["cardkey_log"] = new List<QuizVariant>
            {
                new QuizVariant {
                    // 출처: 1교시 제13조 개인정보 파기 - 인쇄물은 파쇄/소각
                    question = "개인정보가 담긴 출력물을 더 이상 쓰지 않게 됐다. 올바른 파기 방법은?",
                    options = new[] {
                        "쓰레기통에 그냥 버린다",
                        "파쇄(파쇄기) 또는 소각으로 복원 불가능하게 폐기한다",
                        "책상 서랍에 넣어둔다",
                        "재활용 종이함에 넣는다"
                    },
                    correctIndex = 1,
                    successClue = "탕비실 쓰레기통에서 미파쇄 기밀 출력물 발견. 정보 유출 위험.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제6조 접근통제 - 세션 타임아웃, 화면보호기는 접속차단 아님
                    question = "개인정보처리시스템에 로그인한 채 자리를 비운다. 올바른 조치는?",
                    options = new[] {
                        "잠깐이니 그대로 두고 다녀온다",
                        "화면 잠금(Win+L) 또는 시스템 접속을 차단하고 자리를 뜬다",
                        "모니터 전원만 끈다",
                        "화면보호기만 켜두면 접속이 차단된다"
                    },
                    correctIndex = 1,
                    successClue = "점심시간 직원 부재 중 PC 미잠금 다수. 내부 자료 무단 열람 정황.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제10조 물리적 안전조치 - 전산실 출입통제 / 사회공학 무단진입
                    question = "낯선 외부인이 사원증 없이 보안 구역 문에 바짝 붙어 함께 들어오려 한다(테일게이팅). 올바른 행동은?",
                    options = new[] {
                        "친절하게 문을 열어준다 (배달원 같으니)",
                        "정중히 막고 안내 데스크로 안내한 뒤 보안팀에 보고한다",
                        "신원만 묻고 들여보낸다",
                        "본인 카드로 열어주고 자리를 비운다"
                    },
                    correctIndex = 1,
                    successClue = "사회공학 무단 진입 시도 흔적. CCTV에 미사원 진입 다수 기록됨.",
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
