-- Criação das Tabelas
CREATE TABLE Time (
    TimeID INT IDENTITY (100,10) PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL,
    Apelido VARCHAR(50),
    DataCriacao DATE,
    Pontos INT DEFAULT 0,
    GolsMarcados INT DEFAULT 0,
    GolsSofridos INT DEFAULT 0
);

CREATE TABLE Jogo (
    JogoID INT IDENTITY (1,1) PRIMARY KEY,
    TimeCasaID INT,
    TimeVisitanteID INT,
    GolsCasa INT DEFAULT 0,
    GolsVisitante INT DEFAULT 0,
    DataJogo DATE,
    FOREIGN KEY (TimeCasaID) REFERENCES Time(TimeID),
    FOREIGN KEY (TimeVisitanteID) REFERENCES Time(TimeID)
);
GO

-- Criação da Stored Procedure para Atualizar Pontuação
CREATE PROCEDURE AtualizarPontuacao
    @TimeCasaID INT,
    @TimeVisitanteID INT,
    @GolsCasa INT,
    @GolsVisitante INT
AS
BEGIN
    -- Atualiza pontos dos times envolvidos
    IF @GolsCasa = @GolsVisitante
    BEGIN
        UPDATE Time SET Pontos = Pontos + 1 WHERE TimeID = @TimeCasaID;
        UPDATE Time SET Pontos = Pontos + 1 WHERE TimeID = @TimeVisitanteID;
    END
    ELSE IF @GolsCasa > @GolsVisitante
    BEGIN
        UPDATE Time SET Pontos = Pontos + 3 WHERE TimeID = @TimeCasaID;
    END
    ELSE
    BEGIN
        UPDATE Time SET Pontos = Pontos + 5 WHERE TimeID = @TimeVisitanteID;
    END

    -- Atualiza gols marcados e sofridos
    UPDATE Time
    SET GolsMarcados = GolsMarcados + @GolsCasa, GolsSofridos = GolsSofridos + @GolsVisitante
    WHERE TimeID = @TimeCasaID;

    UPDATE Time
    SET GolsMarcados = GolsMarcados + @GolsVisitante, GolsSofridos = GolsSofridos + @GolsCasa
    WHERE TimeID = @TimeVisitanteID;
END;
GO

-- Stored Procedure para Gerar Jogos
CREATE PROCEDURE GerarJogos
AS
BEGIN
    DECLARE @TimeCasaID INT, @TimeVisitanteID INT
    DECLARE @GolsCasa INT, @GolsVisitante INT
    DECLARE @DataJogo DATE = GETDATE()

    -- Cursores para iterar sobre os times
    DECLARE TimeCursor CURSOR FOR
    SELECT TimeID FROM Time;

    DECLARE VisitanteCursor CURSOR FOR
    SELECT TimeID FROM Time;

    OPEN TimeCursor
    FETCH NEXT FROM TimeCursor INTO @TimeCasaID

    WHILE @@FETCH_STATUS = 0
    BEGIN
        OPEN VisitanteCursor
        FETCH NEXT FROM VisitanteCursor INTO @TimeVisitanteID

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Evita criar jogo onde TimeCasaID é igual a TimeVisitanteID
            IF @TimeCasaID < @TimeVisitanteID
            BEGIN
                -- Gera gols aleatórios para a partida de "ida"
                SET @GolsCasa = ABS(CHECKSUM(NEWID())) % 5 -- Usei para gerar (NEWID) números aletórios positivos (ABS) de gols entre 0 e 4 (%5)
                SET @GolsVisitante = ABS(CHECKSUM(NEWID())) % 5

                -- Insere jogo de "ida"
                INSERT INTO Jogo (TimeCasaID, TimeVisitanteID, GolsCasa, GolsVisitante, DataJogo)
                VALUES (@TimeCasaID, @TimeVisitanteID, @GolsCasa, @GolsVisitante, @DataJogo)

                -- Atualiza pontuação dos times para o jogo de "ida"
                EXEC AtualizarPontuacao @TimeCasaID, @TimeVisitanteID, @GolsCasa, @GolsVisitante

                -- Gera gols aleatórios para a partida de "volta"
                SET @GolsCasa = ABS(CHECKSUM(NEWID())) % 5
                SET @GolsVisitante = ABS(CHECKSUM(NEWID())) % 5

                -- Insere jogo de volta
                INSERT INTO Jogo (TimeCasaID, TimeVisitanteID, GolsCasa, GolsVisitante, DataJogo)
                VALUES (@TimeVisitanteID, @TimeCasaID, @GolsVisitante, @GolsCasa, DATEADD(DAY, 7, @DataJogo))

                -- Atualiza pontuação dos times para o jogo de "volta"
                EXEC AtualizarPontuacao @TimeVisitanteID, @TimeCasaID, @GolsVisitante, @GolsCasa
            END

            FETCH NEXT FROM VisitanteCursor INTO @TimeVisitanteID
        END

        CLOSE VisitanteCursor
        FETCH NEXT FROM TimeCursor INTO @TimeCasaID
    END

    CLOSE TimeCursor
    DEALLOCATE TimeCursor
    DEALLOCATE VisitanteCursor
END;
GO


-- Queries para responder às perguntas solicitadas

-- Quem é o campeão no final do campeonato?
SELECT TOP 1 Nome, Pontos
FROM Time
ORDER BY Pontos DESC, GolsMarcados - GolsSofridos DESC;


-- Como faremos para verificar os 5 primeiros times do campeonato?
SELECT TOP 5 Nome, Pontos
FROM Time
ORDER BY Pontos DESC, GolsMarcados - GolsSofridos DESC;


-- Quem é o time que mais fez gols no campeonato?
SELECT TOP 1 Nome, GolsMarcados
FROM Time
ORDER BY GolsMarcados DESC;


-- Quem é que tomou mais gols no campeonato?
SELECT TOP 1 Nome, GolsSofridos
FROM Time
ORDER BY GolsSofridos DESC;


-- Qual é o jogo que teve mais gols?
SELECT TOP 1 
    JogoID,
    (SELECT Nome FROM Time WHERE TimeID = TimeCasaID) AS TimeCasa,
    (SELECT Nome FROM Time WHERE TimeID = TimeVisitanteID) AS TimeVisitante,
    GolsCasa,
    GolsVisitante,
    (GolsCasa + GolsVisitante) AS TotalGols
FROM 
    Jogo
ORDER BY 
    TotalGols DESC;


-- Qual é o maior número de gols que cada time fez em um único jogo?
SELECT 
    TimeID,
    (SELECT Nome FROM Time WHERE TimeID = Combined.TimeID) AS NomeTime,
    MAX(Gols) AS MaxGols
FROM 
    (
    SELECT 
        TimeCasaID AS TimeID,
        GolsCasa AS Gols
    FROM 
        Jogo
    UNION ALL
    SELECT 
        TimeVisitanteID AS TimeID,
        GolsVisitante AS Gols
    FROM 
        Jogo
    ) AS Combined
GROUP BY 
    TimeID;


-- Comando para Inserir Rapidamente pelo Banco (Assim não precisa ficar digitando =D )
INSERT INTO Time (Nome, Apelido, DataCriacao) VALUES
('Leões Vermelhos', 'Leões', '2020-01-15'),
('Lobos Cinzentos', 'Lobos', '2019-03-20'),
('Corvos Azuis', 'Corvos', '2018-05-10'),
('Serpentes Verdes', 'Serpentes', '2021-07-25'),
('Texugos Dourados', 'Texugos', '2017-09-30');

SELECT * FROM TIME
SELECT * FROM JOGO

-- Comando para remover e zerar os contadores se der ruim =D
DELETE FROM TIME
DELETE FROM JOGO
DBCC CHECKIDENT ('Jogo', RESEED, 0);
DBCC CHECKIDENT ('Time', RESEED, 100)