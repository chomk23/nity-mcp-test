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
                        "주인을 찾아주려고 내 컴퓨터에 꽂아 내용물을 먼저 확인한다",
                        "손대지 않고 보안팀에 신고·전달한다",
                        "백신이 설치된 PC라면 꽂아서 검사해봐도 된다",
                        "회사 물건이 아니니 분실물함에 넣어 둔다"
                    },
                    correctIndex = 1,
                    successClue = "USB에 새벽 2시 연구실 서버 백업 데이터가 들어있다. 누군가 빼돌리려 했다.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제6조 접근통제 - Endpoint DLP(USB·보조저장매체 통제)
                    question = "개인정보가 든 보조저장매체(USB·외장하드)는 어떻게 관리해야 하나?",
                    options = new[] {
                        "부서원끼리만 쓰는 것이므로 서랍에 넣어두면 충분하다",
                        "암호화해 저장하고 잠금장치 있는 곳에 보관한다",
                        "비밀번호를 걸어두면 아무 곳에나 보관해도 안전하다",
                        "사용 빈도가 높으니 책상 위에 꺼내두고 쓰는 게 낫다"
                    },
                    correctIndex = 1,
                    successClue = "협력사 USB에 평소와 다른 .exe 파일 다수. 사회공학 공격 시도로 추정.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 3교시 사고원인 Top - "퇴사 시 USB로 개인정보 다운로드"(고의유출)
                    question = "퇴사를 앞둔 직원이 고객 명단을 개인 USB로 복사하려 한다. 무엇이 문제인가?",
                    options = new[] {
                        "본인이 직접 만들고 관리한 자료라면 가져가도 된다",
                        "권한 없는 개인정보 반출 — 금지·신고 대상이다",
                        "회사에 손해가 없도록 암호화해서 가져가면 괜찮다",
                        "인수인계 목적이라고 말해두면 문제없다"
                    },
                    correctIndex = 1,
                    successClue = "개인 USB 무단 반출 시도 흔적. 자료 외부 유출 가능성 시사.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제6조 접근통제 - 외부 접속 시 안전한 접속·인증수단(VPN 등)
                    question = "출장 중 카페 공공 와이파이로 사내 시스템에 접속해야 한다. 안전한 방법은?",
                    options = new[] {
                        "카페 와이파이도 비밀번호가 걸려 있으므로 그대로 접속한다",
                        "회사 VPN(가상사설망)을 통해서만 접속한다",
                        "주소가 https로 시작하는 사이트라면 어떤 망에서든 안전하다",
                        "휴대폰 테더링은 이동통신망이라 무조건 안전하다"
                    },
                    correctIndex = 1,
                    successClue = "출장 직원 노트북에서 VPN 우회 흔적. 외부 망에서 사내 접근 시도.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제13조 개인정보 파기 - 인쇄물은 파쇄/소각
                    question = "개인정보가 담긴 출력물을 더 이상 쓰지 않게 됐다. 올바른 파기 방법은?",
                    options = new[] {
                        "손으로 잘게 찢어 일반 쓰레기통에 나눠 버린다",
                        "파쇄기로 파쇄하거나 소각해 폐기한다",
                        "뒷면을 이면지로 재활용한 뒤 종이함에 넣는다",
                        "개인정보 면이 안 보이게 접어서 재활용함에 넣는다"
                    },
                    correctIndex = 1,
                    successClue = "탕비실 쓰레기통에서 미파쇄 기밀 출력물 발견. 정보 유출 위험.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제6조 접근통제 - 세션 타임아웃, 화면보호기는 접속차단 아님
                    question = "개인정보처리시스템에 로그인한 채 자리를 비운다. 올바른 조치는?",
                    options = new[] {
                        "잠깐이면 그대로 두고 다녀와도 된다",
                        "화면 잠금(Win+L) 후 자리를 뜬다",
                        "모니터 전원을 꺼두면 화면이 안 보이므로 충분하다",
                        "화면보호기가 곧 켜지므로 따로 잠글 필요 없다"
                    },
                    correctIndex = 1,
                    successClue = "점심시간 직원 부재 중 PC 미잠금 다수. 내부 자료 무단 열람 정황.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제6조 - 노출·공유설정 제한 / 3교시 - 검색엔진 노출 사고
                    question = "회사 기밀 PDF를 개인 클라우드(드롭박스 등)에 백업하려 한다. 무엇이 문제인가?",
                    options = new[] {
                        "백업은 많을수록 좋으니 개인 클라우드도 함께 쓴다",
                        "금지 — 승인된 사내 시스템에만 저장한다",
                        "비밀번호를 건 zip으로 압축해 올리면 안전하다",
                        "링크를 비공개로 설정하면 검색에 안 걸리므로 괜찮다"
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
                    // 출처: 1교시 제8조 접속기록 보관·점검 - 접속기록은 위·변조 방지·보존
                    question = "관리자 권한으로 접근한 비정상 접속기록(로그)을 발견했다. 우선 조치는?",
                    options = new[] {
                        "시스템 오류일 수 있으니 며칠 더 지켜본다",
                        "혼선을 막기 위해 문제된 로그를 정리(삭제)해 둔다",
                        "로그를 보존한 채 보안팀에 즉시 보고한다",
                        "해당 계정 사용자에게 개인적으로 먼저 물어본다"
                    },
                    correctIndex = 2,
                    successClue = "비밀번호 평문 저장 + 비정상 관리자 접근. 내부자 소행 가능성.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제8조 접속기록 점검 - 업무시간 외 대량 다운로드는 비정상 행위
                    question = "한 계정이 새벽에 외부 IP로 대용량 개인정보를 다운로드·전송 중이다. 올바른 대응은?",
                    options = new[] {
                        "업무용 백업일 수 있으니 전송이 끝날 때까지 기다린다",
                        "즉시 차단하고 접속기록을 분석해 사유를 확인한다",
                        "내 계정에서 벌어진 일이 아니므로 신경 쓰지 않는다",
                        "다음 날 출근한 담당자에게 구두로 전달한다"
                    },
                    correctIndex = 1,
                    successClue = "어제 23:47, 미식별 외부 IP로 1.2GB 전송. 발신 단말은 사내 네트워크 단말.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 3교시 사고원인 - 임원 사칭 피싱메일(스피어피싱) / 발신 도메인 위조
                    question = "회사 임원을 사칭한 메일에 .zip 첨부와 '긴급 송금' 요청이 있다. 올바른 행동은?",
                    options = new[] {
                        "임원 지시는 긴급하므로 첨부파일부터 열어 확인한다",
                        "발신 도메인 철자를 확인하고 의심되면 보안팀에 신고한다",
                        "회사 메일 시스템이 걸러줬을 테니 첨부는 안전하다",
                        "판단이 어려우니 동료들에게 전달해 같이 열어본다"
                    },
                    correctIndex = 1,
                    successClue = "임원 사칭 피싱 메일 다수. 발신 도메인이 미세하게 다름(o → 0).",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 2교시 크리덴셜 스터핑 사례 - 유출된 ID/PW 무작위 대입 공격
                    question = "여러 사이트에 같은 비밀번호를 재사용하면 위험한 이유는? (크리덴셜 스터핑)",
                    options = new[] {
                        "외우기 쉬워 계정이 잠길 일이 없으니 오히려 낫다",
                        "한 곳에서 유출되면 다른 사이트까지 연쇄적으로 뚫린다",
                        "충분히 길고 복잡한 비밀번호라면 재사용해도 안전하다",
                        "사내 시스템끼리는 방화벽 안이라 같아도 괜찮다"
                    },
                    correctIndex = 1,
                    successClue = "직원 다수가 사내·사외 공통 비밀번호 사용. 외부 유출이 사내 침입으로 직결됨.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 2교시 클라우드 2차인증 미적용 사례 / 1교시 제5조 인증수단(OTP)
                    question = "외부에서 개인정보처리시스템에 접속할 때 ID·비밀번호만 쓰고 있다. 무엇이 필요한가?",
                    options = new[] {
                        "비밀번호를 주기적으로 바꾸면 2차 인증은 필요 없다",
                        "OTP·인증서 등 2차 인증을 추가로 적용한다",
                        "관리자 계정만 2차 인증하면 일반 직원은 불필요하다",
                        "허용된 사내 IP에서만 접속하므로 추가 인증은 과하다"
                    },
                    correctIndex = 1,
                    successClue = "주요 계정 다수 2FA 미설정. 비밀번호 유출 시 즉시 내부망 침입 가능 상태.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제5조 - 일정 횟수 이상 인증 실패 시 접근 제한
                    question = "한 계정에 짧은 시간 비밀번호 입력 실패가 수십 번 반복된다(무차별 대입 정황). 올바른 통제는?",
                    options = new[] {
                        "본인이 비밀번호를 잊었을 수 있으니 계속 시도하게 둔다",
                        "일정 횟수 이상 실패하면 계정을 잠근다",
                        "실패를 줄이도록 비밀번호를 더 단순하게 바꿔준다",
                        "로그가 지저분해지므로 실패 기록을 지워 정리한다"
                    },
                    correctIndex = 1,
                    successClue = "직원 계정에 해외 IP 로그인 시도 다수. 자격증명 유출 정황.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제7조 암호화 - 비밀번호는 복호화 불가한 일방향 암호화(SHA-256)
                    question = "개발자가 비밀번호를 소스코드에 평문으로 하드코딩해 두었다. 올바른 저장 방식은?",
                    options = new[] {
                        "소스코드는 사내에서만 보므로 평문이어도 문제없다",
                        "일방향 해시(SHA-256 등)로 암호화해 저장한다",
                        "나중에 복호화할 수 있게 양방향 암호화로 저장한다",
                        "주석 처리로 눈에 안 띄게 가려두면 안전하다"
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
                    // 출처: 1교시 제5조 접근권한 관리 - 인사이동 시 지체없이 권한 변경/말소
                    question = "퇴직·전보로 업무가 바뀐 직원의 출입·시스템 권한이 그대로 남아있다. 올바른 조치는?",
                    options = new[] {
                        "복직하거나 다시 쓸 수 있으니 당분간 살려 둔다",
                        "즉시 권한을 변경·말소하고 내역을 기록한다",
                        "본인이 반납을 요청할 때까지 기다린다",
                        "연말 정기 점검 때 한꺼번에 정리하면 된다"
                    },
                    correctIndex = 1,
                    successClue = "복제 카드키가 새벽 1~3시 보안 구역 출입에 사용됐다.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제5조 - 접근권한은 업무에 필요한 최소 범위로 차등 부여
                    question = "상담 직원 전원에게 고객정보 전체 다운로드 권한이 부여돼 있다. 무엇이 문제인가?",
                    options = new[] {
                        "업무 요청이 몰릴 수 있으니 전원에게 주는 게 효율적이다",
                        "과다 부여 — 업무별 최소 범위로 차등 부여해야 한다",
                        "권한이 많을수록 장애 대응이 빨라져 오히려 안전하다",
                        "관리자가 결재해 정한 사항이므로 문제없다"
                    },
                    correctIndex = 1,
                    successClue = "CCTV 점검 사유로 30분간 꺼졌으나 점검 일정 없음. 의도적 비활성 정황.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 1교시 제10조 물리적 안전조치 - 전산실 출입통제 / 사회공학 무단진입
                    question = "낯선 외부인이 사원증 없이 보안 구역 문에 바짝 붙어 함께 들어오려 한다(테일게이팅). 올바른 행동은?",
                    options = new[] {
                        "배달원으로 보이면 바쁠 테니 문을 잡아준다",
                        "정중히 막고 안내 데스크로 안내한 뒤 보안팀에 알린다",
                        "방문 목적만 물어보고 들여보낸다",
                        "일단 들여보내고 나중에 CCTV로 확인하면 된다"
                    },
                    correctIndex = 1,
                    successClue = "사회공학 무단 진입 시도 흔적. CCTV에 미사원 진입 다수 기록됨.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 3교시 유·노출 시 조치 - 유출 인지 시 지체없이 통지·신고(72시간)
                    question = "고객 개인정보 1천 건 이상이 유출된 사실을 인지했다. 올바른 절차는?",
                    options = new[] {
                        "내부 조사를 완전히 끝낸 뒤에 천천히 알린다",
                        "지체 없이 통지하고 72시간 내 보호위원회·KISA에 신고한다",
                        "피해 신고가 접수되기 전까지는 알릴 의무가 없다",
                        "규모가 작아 보이면 통지를 생략해도 된다"
                    },
                    correctIndex = 1,
                    successClue = "직원 카드 분실 신고 지연 다수. 분실 카드가 외부 침입에 악용된 흔적.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 2교시 수탁자 부주의 유출 - 게시판 잘못 게시 / 위탁자 책임
                    question = "위탁업체 직원이 실수로 회원 명단 엑셀을 홈페이지 게시판에 올렸다. 핵심 교훈은?",
                    options = new[] {
                        "계약서에 책임 조항이 있으면 위탁사는 완전히 면책된다",
                        "위탁자에게도 책임이 있다 — 수탁자 관리·감독이 필수다",
                        "수탁사 직원 개인의 실수이므로 그 개인의 책임이다",
                        "게시글을 빨리 지우면 신고 없이 마무리해도 된다"
                    },
                    correctIndex = 1,
                    successClue = "위장 USB 충전기 + 키로거가 든 익명 택배. 명백한 사회공학 공격 시도.",
                    clueReward = 2
                },
                new QuizVariant {
                    // 출처: 3교시 노출 사례 - 엑셀 숨김 시트/행·열/배경색 글자에 개인정보 잔존
                    question = "엑셀을 외부에 공유하기 전 점검할 사항으로 가장 적절한 것은?",
                    options = new[] {
                        "화면에 보이는 표만 깨끗하면 그대로 보내도 된다",
                        "숨긴 시트·행·열과 메모까지 확인해 삭제·마스킹한다",
                        "글자색을 배경색과 같게 바꿔두면 노출되지 않는다",
                        "시트 보호 비밀번호를 걸어두면 내용 확인은 불필요하다"
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
