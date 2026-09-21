@echo off
call "%~dp0eng\common\build.cmd" -restore -build -warnNotAsError "CS0618;CS8600;CS8604;CS8605;CS8618;CS8622;CS8625;CS8767;XC0022;XC0025" %*
