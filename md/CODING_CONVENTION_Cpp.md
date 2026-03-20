# 요약
```cpp
namespace WsiuEditor::Example
{
    // 전역 함수 템플릿: 
    template <typename _value_type>
    void GlobalTemplateFunction(_value_type&& value)
    {
        // ...
    }

    // 클래스 템플릿: 
    template <typename _value_type, typename _cleaner_type = default_cleaner>
    class ExamplePool : public ImguiBase
    {
    public:
        using value_type   = _value_type;
        using cleaner_type = _cleaner_type;

        static constexpr int MAX_COUNT = 100;

        // 생성자: 
        ExamplePool(Engine* engine, uint64_t id) 
            : 
            ImguiBase(engine, id), 
            _myId(0), 
            _isActive(false)
        {
            _myId = ExamplePool::globalCounter;
            ExamplePool::globalCounter++;
        }

        // 멤버 함수 템플릿: 클래스 인자와 구분하기 위해 __ 
        template <typename __value_type>
        void Update(__value_type&& value, float deltaTime) 
        {
            // 명시적 캐스팅: 축소 변환 시 static_cast 강제
            int deltaInt = static_cast<int>(deltaTime);
            
            if (deltaInt == 100)
            {
                _myId = deltaInt;
            }
        }

    private:
        static int globalCounter; 
        int _myId;                
        bool _isActive;           
    };
}
```

# 1. 명명 규칙

| 분류 | 규칙 | 예시 | 비고 |
| :--- | :--- | :--- | :--- |
| **클래스** | PascalCase | EditorManager | |
| **인터페이스** | **I** + PascalCase | IEditor | struct 권장 |
| **비공개 멤버** | **_** + camelCase | _activeEditors | 객체 내부 상태 |
| **비공개 정적** | camelCase | editorRegistry | 클래스 공유 상태 |
| **공개 멤버** | PascalCase | IsVisible | 구조체 필드 등 |
| **지역 변수** | camelCase | currentIndex | 매개변수 포함 |
| **불리언 변수** | **is, can, was** 등 | _isActive | 접두사 필수 |
| **상수/매크로** | UPPER_SNAKE_CASE | MAX_SIZE | constexpr 권장 |
| **템플릿 인자** | **스코프 기반 _** | `_type`, `__type` | 중첩 깊이에 따라 추가 |

---

# 2. 코드 스타일

### 중괄호 및 줄 바꿈
* **Allman Style 적용**
    * 모든 중괄호(`{ }`)는 반드시 **새 줄**에서 시작합니다.
    * `if`, `for`, `while` 문 등은 실행 문장이 하나더라도 **절대로 한 줄로 작성하지 않습니다.** (디버깅 편의성 최우선)

### 비교 연산
* **Literal Right (리터럴 우측 배치)**
    * 비교 시 변수를 좌측에, 리터럴(`nullptr`, `true`, `0` 등)을 **우측**에 배치합니다.
    * `bool` 비교 시 생략하지 않고 `== true` 또는 `== false`를 명시합니다.

### 정적 멤버 접근
* **Explicit Static Access**
    * 클래스 내부에서 `static` 멤버에 접근할 때도 반드시 **`ClassName::Member`** 형태로 호출합니다.

---

# 3. 작성 요령 및 안전 규칙

### 헤더 구현 최소화 (Header Minimalism)
* **가독성 및 디버깅**: 헤더 파일(`.h`)에는 클래스 정의와 선언을 위주로 작성하며, 실제 로직 구현은 가급적 소스 파일(`.cpp`)로 분리합니다.
* **예외**: 템플릿 클래스나 극도로 단순한 Getter/Setter를 제외하고는 헤더 내 인라인 구현을 지양합니다. 이는 컴파일 속도 향상과 심볼 추적의 명확성을 위함입니다.

### 스코프 기반 템플릿 인자 명명
* **계층별 접두사 분리**: 상위 스코프와 이름이 겹치지 않도록 중첩 깊이에 따라 `_`를 추가합니다.
    * **1단계 (전역/클래스)**: `_value_type`
    * **2단계 (멤버 함수)**: `__value_type`
* **타입 별칭(using) 필수**: 클래스 템플릿 인자는 반드시 클래스 내에서 `using`으로 재정의하여 사용합니다.

### 명시적 형변환 및 의도 명시
* **C++ Style Casting 강제**: `static_cast`, `dynamic_cast`, `reinterpret_cast`를 사용하며, C 스타일 캐스팅은 금지합니다.
* **명시적 호출 원칙**: 암시적 형변환에 의존하지 않고, 호출하는 쪽에서 명확한 타입을 인자로 넘기도록 작성합니다.
* **축소 변환 명시**: 데이터 손실이 발생하는 변환은 반드시 `static_cast`를 거칩니다.

### 디버깅 가독성
* 연속된 멤버 액세스 연산자(`->`, `.`) 호출보다는 중간 단계를 지역 변수에 할당하여 조사식에서 값을 확인하기 쉽게 작성합니다.
