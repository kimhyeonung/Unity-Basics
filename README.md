# Forest Warden

Unity 6000.0.74f1에서 실행하는 2D 슈팅 게임입니다.

`Assets/Scenes/Forest.unity`를 열고 Play를 누르면 됩니다. 시작 버튼이나 Enter로 게임을 시작합니다.

체력, 점수, 웨이브, 조작 안내와 시작 버튼은 게임 시작 전부터 화면에 표시됩니다. 시작 버튼은 게임 중에 Restart 버튼으로 바뀝니다.

| 조작 | 키 |
| --- | --- |
| 이동 | WASD 또는 방향키 |
| 조준 | 마우스 |
| 발사 | 마우스 왼쪽 버튼 또는 Space |
| 처음부터 다시 시작 | R |

적은 원형, 격자, 위아래 두 줄로 등장합니다. 각 웨이브에 12마리가 나오고 3번째 웨이브까지 처치하면 클리어입니다. 적의 체력은 50이며, 처치하면 10점을 얻습니다.

EnemyManager는 적 12마리, MissileManager는 미사일 40개를 미리 만들어 둡니다. 적 사망과 미사일 충돌·수명 종료는 오브젝트를 비활성화하고, 다음 사용 때 체력과 이동 상태를 초기화합니다. HPController는 플레이어와 적의 체력 및 체력바를 관리합니다.

바위 경계, 풀과 이끼 장식, 배경 음악에는 [Free Asset - 2D Handcrafted Art](https://assetstore.unity.com/packages/2d/environments/free-asset-2d-handcrafted-art-117049)를 사용했습니다.

Windows 빌드는 Unity의 Build Profiles에서 생성합니다. 만들어 둔 실행 파일은 `Build/Windows/Forest Warden.exe`입니다.
