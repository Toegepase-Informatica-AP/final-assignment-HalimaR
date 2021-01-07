# VR Trefbal Handleiding

Chauvaux Nico, Collette Cédric, Messiaen Ruben, Van Nueten Wouter en Rahimi Halima

## Inleiding

Voor het vak VR Experience hebben wij als project gekozen om een VR Trefbal spel te maken. Bij dit spel is het de bedoeling dat de speler drie ontwijkers probeert te raken met een bal. Deze ontwijkers zijn AI's die we getraind hebben voor dit project.

## Samenvatting

Met deze handleiding geven wij stap voor stap uitleg hoe men van een leeg Unity project kan bekomen aan ons VR Trefbal spel. Er zal uitgelegd worden welke spelobjecten wij hebben aangemaakt, hoe deze zich kunnen gedragen in de spel omgeving en welke andere installaties er benodigd zijn.

## Methoden

### Installaties

Voor het spel werkende krijgen zijn er verschillende installaties vereist:

- Unity: 2019.4.10f1
- ML-Agents: 0.21.1
- Unity Oculus XR Plugin: 1.4.3
- Anaconda: 4.8.3

### Spelverloop

Bij het eerst inladen van de VR omgeving zal de speler zich bevinden in een sportzaal op een speelveld. De speler zal gecentreerd op een helft van dat speelveld staan. Aan de andere kant van het speelveld bevinden zich drie rode pion achtige firguren. Dit zijn de ontwijkers.

Het doel van het spel is dat de speler de ontwijkers uit schakelt met behulp van een ongelimiteerde hoeveelheid ballen die de speler kan gooien. Zolang de bal de ontwijker eerst raakt zal de ontwijker zijn uit geschakelt. Als de bal de grond eerst raakt en dan pas tegen de ontwijker telt dit dus niet mee. De ontwijkers zullen actief deze ballen proberen ontwijken. Zij hebben in tegenstelling tot de speler wel de vrijheid om zich te verplaatsen op hun zijde van het speelveld.

Tijdens het spel zullen er "power-ups" tevoorschijn komen. Deze power-ups bevinden zich op willekeurige plekken boven het veld. Als de speler een power-up kan raken door er een bal tegen te gooien zal de power-up voor een gelimiteerde tijd geactiveerd worden. Deze power-up zorgt er voor dat de ballen die speler kan gooien groter zijn dan normaal. Met deze power-up zal het dus makkelijker zijn om de ontwijkers te raken.

Het spel duurt maximaal 120 seconden. Als de speler binnen deze tijd de drie ontwijkers krijgt uitgeschakelt is het spel gewonnen. Als de tijd op is voor alle ontwijkers zijn uitgeschakelt is het spel verloren en moet de speler opnieuw beginnen.

### Observaties, acties en beloningen

#### Observaties

De ontwijker kan verscheidene dingen observeren in het speelveld. Dit zijn de verschillende observaties die de ontwijker kan hebben:

- PlayingGround: Het deel van het speelveld waar de ontwijkers op moeten blijven.
- Player: De ontwijker kan de speler zien, zodat hij weet waar de ballen vandaan komen.
- Ball: De ontwijker kan de ballen zien aankomen, waardoor hij deze kan ontwijken.
- Dodger: De ontwijkers kunnen ook elkaar zien, zodat ze kunnen vermijden dat ze tegen elkaar oplopen.
- Ground: De ontwijkers kunnen de grond zien van de rest van het veld dat geen speelveld is, zodat ze kunnen vermijden daarop te lopen.

Dit zijn eveneens de tags die de andere gameobjecten bevatten zodat de ontwijker ze kan herkennen.

#### Acties

De ontwijker kan rondbewegen en springen, zodat hij de bal kan ontwijken. De speler zelf kan niet bewegen, maar kan wel de bal gooien naar de ontwijkers. De speler kan ook de bal gooien naar de power-ups om zo grotere ballen te krijgen.

#### Beloningen

De ontwijker krijgt verscheidene beloningen en straffen voor de acties die hij onderneemt tijdens het spelverloop:

- Geraakt door bal: - 0.5
- Op het speelveld: + 0.01
- Buiten het speelveld van de ontwijkers: - 0.5

![RayPerceptions](./Afbeeldingen/RayPerceptions.jpg)

### Beschrijving en opbouw objecten

#### Omgeving

![Speelveld](./Afbeeldingen/Speelveld.jpg)

De spelomgeving is een zaal met tribunes en toeschouwers. De toeschouwers hebben geen invloed op het spel. Alleen de grond (rood), speelveld (groen) en de middenlijn die het speelveld verdeelt in twee delen (wit) zijn van belang voor de ontwijker. De zaal zijn vier Panes die een rode material bevatten. Er zijn ook acht capsules aanwezig die een spotlight hebben om de zaal te verlichten. Het speelveld bestaat uit twee aparte platte Cubes. De tribunes zijn opgebouwd uit verscheidene Cubes die op elkaar gestapeld zijn en de toeschouwers zijn groene Spheres met twee paar witte en zwarte Spheres die ogen vormen. Ook is er een onzichtbare Cube Box Collider die ervoor zorgt dat de power-ups binnen zijn grenzen kan spawnen. De deuren en tekst op de muur zijn optioneel.

Het eerste wat men moet doen is het maken van een environment de script:

1. Selecteer het speelveld in dit geval zal dit de Gym zijn.
2. In de inspector klikt men op `Add Component`.
3. In de components lijst klikt men op `New Script` (dit bevind zich onder aan)
4. geef de script de naam "Environment"
5. dan klikt men op `Create and Add`

laten we nu de script aanpassen. In de Unity Project window dubbel klikt men op `Environment.cs`, dit opent de script in de code editor. We beginnen met enkele object-variabelen toetevoegen. We zullen enkele object-variabelen overlopen de anderen zullen uitgelegd worden verder in de tutorial.

```cs (Environment.cs)
    public const float MAXTIME = 120f;
    public float ballAverageSpawnTimer = 2f;
    public bool TrainingMode = true;
    public bool ballHasBeenTakenNonTraining;
    public bool powerUpBall = false;
    public GameObject ballPrefab;
    public GameObject dodgerPrefab;
    public GameObject powerUpSpawnLocation;
    public GameObject powerUpPrefab;
    public List<GameObject> powerUpList;
    public List<Dodger> dodgersList;

    private const float POWERUP_SPAWNTIMER = 15f;
    private float episodeTime = MAXTIME;
    private float ballRespawnTime;
    private float currentScore = 0f;
    private float currentUpgradeTimer = POWERUP_SPAWNTIMER;
    private float largeScale = 2f;
    private float largeTimer = 10f;
    private bool throwing = true;
    private bool spawnDodgers;
    private bool spawningPowerups = true;
    private System.Random random = new System.Random();
    private GameObject ballSpawnpointNonTraining;
    private GameObject balls; 
    private GameObject dodgers;
    private Vector3 standardPositionDL;
    private Vector3 standardPositionDM;
    private Vector3 standardPositionDR;
    private TextMeshPro scoreboard;
    private BoxCollider powerUpSpawnBox;
```

We beginnen met enkele publieke object-variabelen. De float `MAXTIME` toont aan hoelang elke episode zal duren. De `ballAverageSpawnTimer` geeft weer hoelang het duurt voor een ball spawnt. Dan zijn er vier GameObjecten daar moet men de prefabs van de gameobjecten koppelen, dan heeft men ook nog twee lijsten `powerUpList` en `dodgersList`.

De volgende variablen zijn de private objecten. Er zijn enkele float objecten:

- `POWERUP_SPAWNTIMER` geeft aan hoelang het duurt vooralleer een power-up zal spawnen.
- `episodeTime` toont hoelang elke episode zal duren.
- `ballRespawnTime` geeft aan hoelang het duurt vooralleer een bal zal spawnen.
- `currentScore` toont de score.
- `currentUpgradeTimer` wordt telkens terug
- `largeScale`
- `largeTimer`

Dan zijn er ook nog drie bools:

- `throwing`
- `spawnDodgers`
- `spawningPowerups`

#### Speler

![Speler](./Afbeeldingen/Speler.jpg)

Het speler-gameobject heeft op zich niet veel nut, maar wordt gebruikt bij de trainingen van de AI om zo het niet te laten lijken alsof de ballen uit het niets verschijnen. De speler is een blauwe Capsule met geen speciale eigenschappen.

#### Ontwijker

![Ontwijker](./Afbeeldingen/Ontwijker.jpg)

De ontwijker is het object waarop de AI zal worden toegepast. Deze zal aan de hand van de beschikbare acties, beloningen en observaties zo lang mogelijk proberen te overleven en de ballen te ontwijken.
Deze bestaat uit een Capsule als lichaam met één grote witte Sphere als oog en verschillende kleinere zwarte Spheres als de iris van het oog. De zwarte irissen bevatten RayPerceptions die de andere gameobjecten kan waarnemen. Ook is er een plattere zwarte Sphere die dient als wenkbrauw.

#### Bal

![Bal](./Afbeeldingen/Bal.jpg)

De bal is het voorwerp dat de speler kan gooien om de ontwijkers en de power-ups te kunnen raken. De bal is een normale oranje/gele Sphere. Deze bevat ook een geluidsbestand die afspeelt van zodra de bal iets raakt.

#### Power-up

![Bal](./Afbeeldingen/Power-up.jpg)

De power-up zorgt ervoor dat de ballen die gegooid worden gedurende een korte tijd groter worden. Zo heeft de speler meer kans om de ontwijkers te raken. Deze gebruikt de bal prefab samen met een +-vormig gameobject dat bestaat uit twee langwerpige Cubes.

### Beschrijving gedragingen van de objecten

#### Omgeving gedragingen

Het speelveld zelf zal nergens op reageren of heeft geen acties die het kan ondernemen. Het speelveld en de grond krijgen wel verschillende tags mee zodat de ontwijker tijdens de training afgestraft wordt en zodat de ontwijker tijdens het spel weet waar hij wel en niet mag lopen. De onzichtbare Box Collider krijgt ook een tag waardoor de power-ups binnen deze grenzen spawnen.

![Voorbeeld tag](./Afbeeldingen/VoorbeeldTag.jpg)

#### Speler gedragingen

Het object speler heeft geen gedragingen. De speler zelf in de VR-omgeving gaat de mogelijkheid hebben om de ballen te gooien naar de power-ups en de ontwijkers.

#### Ontwijker gedragingen

Ontwijkers gaan aan de hand van het speelveld en de ballen zich zodanig proberen te manouvreren zodat ze de ballen kunnen ontwijken. Hoe langer ze zichzelf in leven kunnen houden, des te meer punten ze krijgen. Voor de AI krijgt de ontwijker een BehaviourParameters script mee. Voor de acties die de ontwijker kan ondernemen krijgt de ontwijker het Dodger script mee.

![Dodger properties](./Afbeeldingen/DodgerProperties.jpg)

#### Bal gedragingen

De bal zorgt ervoor dat de ontwijkers van het speelveld gaan als deze een ontwijker raakt. Ook zorgt de bal ervoor dat de speler de power-ups kan raken zodat deze zijn ballen kan vergroten. Deze bevat dus de BallHit script en een Audio Source.

![Ball properties](./Afbeeldingen/BallProperties.jpg)

#### Power-up gedragingen

De power-ups verschijnen aan de kant van het speelveld van de tegenstanders. Deze power-ups verschijnen om de zoveel tijd en er kunnen er maximaal drie tegelijk zich bevinden op het speelveld.

![Power-up properties](./Afbeeldingen/PowerupProperties.jpg)

### Informatie one-pager

![One-pager](./Afbeeldingen/One-pager.jpg)

Dit is de one-pager dat werd opgemaakt voor de aanvang van het project.

### Afwijking one-pager

Er staat in de one-pager dat de AI's niet hetzelfde reward-systeem gaan hebben. Dit is niet meer het geval, de drie AI's delen hetzelfde reward-systeem, zodat ze elkaar niet zouden tegenwerken.

## Resultaten

## Conclusie

Tijdens dit project hebben we dus een VR Trefbal game gemaakt met behulp van AI en VR.

*Kort overzicht resultaten overlopen (2-3 zinnen)*

*Persoonlijke visie op de resultaten, betekenis van de resultaten*

*Verbeteringen naar de toekomst toe*
