合わゲーム — awagame
================

🚨 **Deprecated** 🚨

Consider using [shiratsu](https://github.com/SnowflakePowered/shiratsu) or the [shiragame 2.0](https://github.com/SnowflakePowered/shiragame) database directly instead.

---

awagame is a small and fast utility to merge together [logiqx ROM Datafiles](www.logiqx.com/Dats/datafile.dtd) into a single queryable SQLite Database or JSON file. Supports No-Intro Parent/Clone XMLs, TOSEC and redump dats.

awagame supports matching against [OpenVGDB](https://github.com/OpenVGDB/OpenVGDB)'s database. The `romID` field will be populated according to the supplied `openvgdb.sqlite`, where you can query against OpenVGDB.

Licensed under the MIT License.
Usage
-------

Simply start awagame in the same directory as a bunch of Datafiles. Specify the output filename and it will quickly build a database.

`awagame --output=filename|-o=filename [--help|-H] [--openvgdb] [--json]`

###--output=filename

awagame requires you to specify the output filename. This has to be the first argument.

###--openvgdb

Check each record to see if it has a romID in OpenVGDB. If it does, it will be recorded in the database. Requires `openvgdb.sqlite` to the in the same directory as `awagame`

###--json

Output to a JSON file rather than an SQLite database. The resulting JSON object has the same schema as the SQLite table, but the records are keyed on the game's SHA1 hash.

###Downloader Usage
awagame can be used to download DAT files from online sources. Use only one parameter at a time.

###--download-redump

Downloads all DAT files from redump.org

###--download-tosec

Downloads the latest TOSEC DAT package from tosecdev.com. This is a rather large file so please wait a bit while it downloads.

###---download-nointro

Downloads all Parent/Clone XML DAT files from datomatic.nointro.org. Due to No Intro throttling after too many downloads, using this option will take a very long time.
Schema
---------

awagame databases have a very simple schema to represent only the data found in the datafiles.

|key|description|
|---|-------------|
|gamename|The name of the parent `game` record of the ROM|
|romname|The ROM file name|
|size|The ROM size|
|crc|The ROM CRC32 hash|
|md5|The ROM MD5 hash|
|sha1|The ROM SHA1 hash (PRIMARY KEY)|
|romID|The OpenVGDB ROM ID (if available)|
