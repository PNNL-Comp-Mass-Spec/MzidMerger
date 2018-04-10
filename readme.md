# MzidMerger

## Overview
Merges multiple .mzid file created by MS-GF+ into a single mzid file.

This is primarily designed for "split-fasta" MS-GF+ searches, where a large protein database was split into multiple .fasta files, and then each one used to search a single spectra file.

## Details

MzidMerger reads in the mzids from MS-GF+ and creates a new mzid file using the contents of the input mzid files

MzidMerger uses PSI_Interface.dll to read the mzid file.

## Syntax

`MzidMerger -inDir "directory path" [-filter "filename filter"] [-out "output file path"] [-maxSpecEValue number] [-keepOnlyBestResults]`

### Required parameters:
`-inDir path` 
* Path to directory containing mzid files to be merged.  If the path has spaces, it must be in quotes.

### Optional parameters:
`-filter:abc*.mzid`
* Filename filter; filenames that match this string will be merged. *.mzid and *.mzid.gz are added if an extension is not present. Use '*' for wildcard matches. (Default: All files ending in .mzid or .mzid.gz).

`-out`
* Filepath/filename of output file; if no path, input directory is used; by default will determine and use the common portion of the input file names.

`-maxSpecEValue`
* Maximum SpecEValue to include in the merged file. Default value includes all results.

`-keepOnlyBestResults`
* If specified, only the best-scoring results for each spectrum are kept.

## Contacts

Written by Bryson Gibbons and Matthew Monroe for the Department of Energy (PNNL, Richland, WA) \
E-mail: proteomics@pnnl.gov \
Website: https://panomics.pnl.gov/ or https://omics.pnl.gov

## License

The MzidMerger is licensed under the Apache License, Version 2.0; 
you may not use this file except in compliance with the License.  You may obtain 
a copy of the License at https://opensource.org/licenses/Apache-2.0
