### 📄 COMMIT_CONVENTION.md

```markdown
# 📝 WSIU Engine Commit Convention Guide

이 문서는 **WSIU 엔진** 프로젝트의 일관된 히스토리 관리를 위한 커밋 메시지 규칙을 정의합니다.

---

### 1. 커밋 메시지 기본 구조
```text
<Type>(<Scope>): <Subject>

[Body - Optional]
[Issue ID - Optional]

```

---

### 2. 타입 (Type)

| 타입 | 성격 | 설명 |
| --- | --- | --- |
| **feature** | 기능 | 새로운 기능 추가 (렌더링 로직, UI 컨트롤 등) |
| **fix** | 수정 | 버그 수정 (메모리 누수, 크래시 해결) |
| **refactor** | 개선 | 기능 변화 없는 내부 로직 및 구조 정리 |
| **docs** | 문서 | README, 주석, 개발 가이드 수정 |
| **chore** | 설정 | 빌드 설정, .gitignore, NuGet 패키지 관리 |
| **perf** | 성능 | 드로우콜 최적화, 메모리 사용량 최적화 |
| **style** | 스타일 | 코드 포맷팅, 네이밍 수정 (로직 변경 없음) |

---

### 3. 영역 (Scope) - WSIU 특화

* **`editor`**: WinUI 3 (C#) 기반의 에디터 인터페이스 관련
* **`core`**: 엔진 메인 루프, 델타 타임, 시스템 관리 로직
* **`render`**: DX11 (C++) 기반의 렌더러 프레임워크 및 셰이더
* **`interop`**: C#과 C++ 사이의 데이터 전달 및 WinRT 연결부
* **`math`**: 벡터, 행렬 등 수학 라이브러리 관련
* **`project`**: 초기 환경 구축 및 전체 프로젝트 설정

---

### 4. 실전 예제 (Examples)

* `chore(project): 초기 WinUI 3 프로젝트 생성 및 .gitignore 설정`
* `feat(editor): MainWindow에 SwapChainPanel 및 Inspector 레이아웃 배치`
* `feat(render): ID3D11Device 및 SwapChain 생성 로직 구현`
* `feat(interop): C# SwapChainPanel 핸들을 C++로 전달하는 Bridge 작성`
* `fix(render): 창 크기 조절 시 렌더 타겟 리사이즈 오류 해결`
* `refactor(core): 엔진 메인 루프 가독성 개선 및 불필요한 참조 제거`

---

### 작성 팁

1. **Subject**: 50자 이내로 요약하며, 가급적 명령형(Add, Fix, Implement)을 권장합니다.
2. **Body**: "무엇을", "왜" 수정했는지 구체적인 설명이 필요한 경우에만 추가합니다.

```

---
