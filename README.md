# Threadloom — 나만의 무기 만들기

제공된 `WeaponSystem1105-Study10.zip`을 기반으로 만든 Unity 2D 무기 과제입니다.
`Assets/Scenes/Threadloom.unity`를 열고 Play로 실행합니다.
제공본의 Unity 버전은 6000.3.11f1이며, 이 환경에 설치된 같은 6000.3 계열의 **6000.3.5f1**에서 임포트·컴파일·Play 검증을 완료했습니다.

## 무기

**실뜨기 직조기(Threadloom)**는 바늘 5개를 시간차로 발사합니다. 바늘은 실을 엮는 곡선으로 전진한 뒤 원을 그리며 회전하고, 처음 발사한 위치로 되감깁니다. 전진·회전·귀환 중 적을 관통하며, 귀환하면 풀로 돌아가 같은 인스턴스를 재사용합니다. 플레이어를 따라가는 자동 유도 무기와 달리 발사 지점을 기준으로 움직여 위치와 조준에 따라 궤적을 설계할 수 있습니다.

| 무기 | 투사체 | 발사 패턴 |
| --- | --- | --- |
| THREADLOOM | 새 `ProjectileControllerThreadloom` | 새 `FirePattern_Weave` |
| NEEDLE BURST | 새 바늘 투사체 | 수업의 `FirePattern_Burst` |
| PRISM FAN | 수업의 `ProjectileController` | 수업의 `FirePattern_Spread` |

## 조작

- WASD / 방향키: 이동
- 마우스: 조준
- 마우스 왼쪽 버튼 / Space: 발사, 누르고 있으면 무기별 간격으로 연사
- Tab: 무기 교체
- R: 표적 재배치 및 체력 회복

화면에는 무기명, 명중·처치 수, 활성 투사체 수만 표시합니다. 영상에 별도 자막이나 조작 설명은 넣지 않습니다.

## 구조

기존 `WeaponController`의 생성·무기 전환·코루틴 발사 흐름은 유지했습니다. 기존 enum에 `Weave = 3`을 추가하고, `FirePatternFactory`에 새 패턴 생성 분기만 추가했습니다. `ProjectilePool`, 기존 투사체와 세 발사 패턴의 구현은 제공본을 유지합니다.

- `Assets/Scripts/FWeapon/Threadloom/ProjectileControllerThreadloom.cs`: `ProjectileController` 상속. 전진·공전·귀환 상태와 궤적, 풀 반환 시 상태 및 잔상 초기화.
- `Assets/Scripts/FWeapon/Threadloom/FirePattern_Weave.cs`: `IFirePattern` 구현. 발사 위치·방향 스냅샷을 기준으로 바늘을 대칭으로 시간차 발사.
- `Assets/Scripts/FWeapon/Threadloom/ThreadloomDefinition.cs`: `WeaponDefinition` 상속. 직조 폭, 공전 반경·회전 수·시간, 귀환 시간을 데이터로 설정.
- `Assets/Scripts/FWeapon/Threadloom/ThreadloomArena.cs`: 조준·입력·연사 간격, 기존 `EnemyController`의 이벤트를 통한 점수, 표적 재생성. 무기 구현과 분리.
- `Assets/Threadloom/Weapons/`: 세 무기의 실제 ScriptableObject 데이터와 실습용 WeaponLibrary.
- `Assets/Editor/ThreadloomSceneBuilder.cs`: 재현 가능한 프리팹·데이터·실습 씬 생성 도구. `Threadloom > Build Assignment Scene` 메뉴로 실행.

`GameTest.unity`와 원래 무기 라이브러리도 남겨 두었으며 새 직조기를 해당 라이브러리에도 추가했습니다. 원본 `Game.unity`는 제공본 그대로 보존한 씬입니다.

## 제출

`Submission/Weapon.zip`: Weapon 폴더에 무기 스크립트와 실행에 쓰이는 지원 스크립트를 모은 압축 파일.
`Submission/Threadloom_Demo.mp4`: 별도 자막 없는 실제 Play 모드 실행 영상.
검사 결과는 `Logs/Threadloom-Validation.txt`, 기존 구조 보존 비교는 `Logs/Threadloom-CorePreservation.txt`에서 확인할 수 있습니다. 검증 및 영상 재생성은 Unity 배치 실행의 `-executeMethod ThreadloomValidation.Run`으로 수행합니다(`-quit` 없이 실행, 완료 후 자동 종료).

최종 실행 검사에서 직조·공전·귀환, 명중 68회·처치 12회, 세 무기의 60개 인스턴스 재사용, 풀 한도 20개, 반환 후 상태·잔상 초기화, 같은 바늘의 재발사 및 영벡터 발사 방지가 통과했습니다. 영상은 1280×720, 30fps, 26초이며 실제 Play 모드 렌더링을 캡처했습니다. 테스트 메서드 호출로 조작을 자동화했으며 물리 키보드 입력 자체를 시험한 영상은 아닙니다.

기존 디자인 패턴 프로젝트와 산출물은 Git 관리에서 제외되는 `.previous-project/20261106-WeaponReplacement/`에 보관했습니다. 현재 프로젝트의 Assets·Packages·ProjectSettings는 제공된 새 프로젝트를 기반으로 교체했습니다.
