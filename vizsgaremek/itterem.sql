-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Gép: 127.0.0.1
-- Létrehozás ideje: 2026. Jan 06. 15:35
-- Kiszolgáló verziója: 10.4.32-MariaDB
-- PHP verzió: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Adatbázis: `itterem`
--

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `hozzavalok`
--

CREATE TABLE `hozzavalok` (
  `id` int(11) NOT NULL,
  `hozzavalo_nev` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_hungarian_ci;

--
-- A tábla adatainak kiíratása `hozzavalok`
--

INSERT INTO `hozzavalok` (`id`, `hozzavalo_nev`) VALUES
(1, 'Hamburger hus'),
(2, 'Hamburger puffancs'),
(3, 'Spegetti tészta'),
(4, 'Spagetti hús');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `jogok`
--

CREATE TABLE `jogok` (
  `id` int(11) NOT NULL,
  `szint` int(1) NOT NULL,
  `nev` varchar(64) NOT NULL,
  `leiras` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_hungarian_ci;

--
-- A tábla adatainak kiíratása `jogok`
--

INSERT INTO `jogok` (`id`, `szint`, `nev`, `leiras`) VALUES
(1, 0, 'Felhasználó', 'Bejelentkezett felhasználó, alap jog'),
(2, 1, 'Felhasználó', 'Alap jogosultság');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `keszetelek`
--

CREATE TABLE `keszetelek` (
  `id` int(11) NOT NULL,
  `nev` varchar(64) NOT NULL,
  `leiras` varchar(100) NOT NULL,
  `elerheto` tinyint(4) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_hungarian_ci;

--
-- A tábla adatainak kiíratása `keszetelek`
--

INSERT INTO `keszetelek` (`id`, `nev`, `leiras`, `elerheto`) VALUES
(1, 'Sima Hamburger', 'Sima Hamburger', 1),
(2, 'Bolognai spagetti', 'Bolognai spagetti', 0);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `keszetel_hozzavalok_kapcsolo`
--

CREATE TABLE `keszetel_hozzavalok_kapcsolo` (
  `keszetel_id` int(11) NOT NULL,
  `hozzavalok_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_hungarian_ci;

--
-- A tábla adatainak kiíratása `keszetel_hozzavalok_kapcsolo`
--

INSERT INTO `keszetel_hozzavalok_kapcsolo` (`keszetel_id`, `hozzavalok_id`) VALUES
(1, 1),
(1, 2),
(2, 3),
(2, 4);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `koretek`
--

CREATE TABLE `koretek` (
  `id` int(11) NOT NULL,
  `nev` varchar(64) NOT NULL,
  `leiras` varchar(100) NOT NULL,
  `elerheto` tinyint(4) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_hungarian_ci;

--
-- A tábla adatainak kiíratása `koretek`
--

INSERT INTO `koretek` (`id`, `nev`, `leiras`, `elerheto`) VALUES
(1, 'testköret1', '', 1);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `menuk`
--

CREATE TABLE `menuk` (
  `id` int(11) NOT NULL,
  `menu_nev` varchar(20) NOT NULL,
  `keszetel_id` int(11) NOT NULL,
  `koret_id` int(11) NOT NULL,
  `udito_id` int(11) NOT NULL,
  `elerheto` tinyint(4) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_hungarian_ci;

--
-- A tábla adatainak kiíratása `menuk`
--

INSERT INTO `menuk` (`id`, `menu_nev`, `keszetel_id`, `koret_id`, `udito_id`, `elerheto`) VALUES
(2, 'Hamburger menu', 1, 1, 1, 1);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `uditok`
--

CREATE TABLE `uditok` (
  `id` int(11) NOT NULL,
  `nev` varchar(20) NOT NULL,
  `elerheto` tinyint(4) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_hungarian_ci;

--
-- A tábla adatainak kiíratása `uditok`
--

INSERT INTO `uditok` (`id`, `nev`, `elerheto`) VALUES
(1, 'Pepsi 0,5L', 0),
(2, 'Nincs ital', 1);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `jogosultsag` int(1) NOT NULL,
  `teljes_nev` varchar(64) NOT NULL,
  `email` varchar(50) NOT NULL,
  `telefonszam` varchar(20) NOT NULL,
  `Hash` varchar(64) NOT NULL,
  `Salt` varchar(64) NOT NULL,
  `aktiv` int(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_hungarian_ci;

--
-- A tábla adatainak kiíratása `users`
--

INSERT INTO `users` (`id`, `jogosultsag`, `teljes_nev`, `email`, `telefonszam`, `Hash`, `Salt`, `aktiv`) VALUES
(6, 1, 'Teszt Felhasználó', 'teszt@teszt.hu', '+36 30 1234567', '620267918570d277815a3700c895dd9afeaf0a38ffe65df63cdb5bcb167da859', 'Yk9SLevKeXLViQoVlAoIflzDCg0jrFjtryFqrPqH6VL2dbDDGMBBfH34fYXK0u2u', 1);

--
-- Indexek a kiírt táblákhoz
--

--
-- A tábla indexei `hozzavalok`
--
ALTER TABLE `hozzavalok`
  ADD PRIMARY KEY (`id`);

--
-- A tábla indexei `jogok`
--
ALTER TABLE `jogok`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `szint` (`szint`);

--
-- A tábla indexei `keszetelek`
--
ALTER TABLE `keszetelek`
  ADD PRIMARY KEY (`id`);

--
-- A tábla indexei `keszetel_hozzavalok_kapcsolo`
--
ALTER TABLE `keszetel_hozzavalok_kapcsolo`
  ADD KEY `keszetel_id` (`keszetel_id`),
  ADD KEY `hozzavalok_id` (`hozzavalok_id`);

--
-- A tábla indexei `koretek`
--
ALTER TABLE `koretek`
  ADD PRIMARY KEY (`id`);

--
-- A tábla indexei `menuk`
--
ALTER TABLE `menuk`
  ADD PRIMARY KEY (`id`),
  ADD KEY `keszetel_id` (`keszetel_id`,`koret_id`),
  ADD KEY `koret_id` (`koret_id`),
  ADD KEY `index` (`udito_id`);

--
-- A tábla indexei `uditok`
--
ALTER TABLE `uditok`
  ADD PRIMARY KEY (`id`);

--
-- A tábla indexei `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `email` (`email`),
  ADD UNIQUE KEY `email_2` (`email`,`telefonszam`),
  ADD KEY `jogosultsag` (`jogosultsag`);

--
-- A kiírt táblák AUTO_INCREMENT értéke
--

--
-- AUTO_INCREMENT a táblához `hozzavalok`
--
ALTER TABLE `hozzavalok`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT a táblához `jogok`
--
ALTER TABLE `jogok`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT a táblához `keszetelek`
--
ALTER TABLE `keszetelek`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT a táblához `koretek`
--
ALTER TABLE `koretek`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT a táblához `menuk`
--
ALTER TABLE `menuk`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT a táblához `uditok`
--
ALTER TABLE `uditok`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT a táblához `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- Megkötések a kiírt táblákhoz
--

--
-- Megkötések a táblához `keszetel_hozzavalok_kapcsolo`
--
ALTER TABLE `keszetel_hozzavalok_kapcsolo`
  ADD CONSTRAINT `keszetel_hozzavalok_kapcsolo_ibfk_1` FOREIGN KEY (`keszetel_id`) REFERENCES `keszetelek` (`id`),
  ADD CONSTRAINT `keszetel_hozzavalok_kapcsolo_ibfk_2` FOREIGN KEY (`hozzavalok_id`) REFERENCES `hozzavalok` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Megkötések a táblához `menuk`
--
ALTER TABLE `menuk`
  ADD CONSTRAINT `menuk_ibfk_1` FOREIGN KEY (`keszetel_id`) REFERENCES `keszetelek` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `menuk_ibfk_2` FOREIGN KEY (`koret_id`) REFERENCES `koretek` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `udito_menu_kapcs` FOREIGN KEY (`udito_id`) REFERENCES `uditok` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Megkötések a táblához `users`
--
ALTER TABLE `users`
  ADD CONSTRAINT `users_ibfk_1` FOREIGN KEY (`jogosultsag`) REFERENCES `jogok` (`szint`) ON DELETE NO ACTION ON UPDATE NO ACTION;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
