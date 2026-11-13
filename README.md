# NetflixClone

Teknologier:

	c#/.NET 10 sdk

	ASP.NET Core Web API

	MongoDB og GridFS

	Docker Compose

	Avalonia UI

	OpenAPI

-------------------------------------------------------------------------------------------------------
Ting NetflixClone kan/funtions:
	
	Stream of afspille videoer

	Oprette nye film, som der kommer i mongodb

	Filmkatalog, den kan vise hvad der allerede er uploaded med meta data 
	
	spole frem og tilbage i en film

	

-----------------------------------------------------------------------------------------------------------	
Installation

note:
	jeg tror ikke jeg behøves at fortælle det, men man kræver ikke mongodb til det her da det kommer igemmen docker installationen

krav for installation:
	.NET 10 SDK, Docker desktop og jetbrain rider
	

1. Start MongoDB

	docker compose up -d

2. Opret MongoDB-forbindelsen

Kør fra mappen NetflixClone.Api

	dotnet user-secrets set "ConnectionStrings:MongoDB" "mongodb://admin:1234@127.0.0.1:27018/?authSource=admin"
	

3. Start APIet
	
	dotnet run --launch-profile http

4. Start desktopapplikationen

	APIet skal stadig køre, mens desktopapplikationen bruges
	
	åbn projektet i Rider, og kør NetflixClone
-----------------------------------------------------------------------------------------------------------

API-dokumentation

	http://localhost:5142/openapi/v1.json

	ligger også i projected som v1.json
--------------------------------------------------------------------------------------------------------------
Arkitektur

APIet bruger repository pattern til filmmetadata og mongodb gridfs til lagring og streaming af videofiler
