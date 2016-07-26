library(qrcode)
ms <- as.character(as.integer(as.POSIXct(Sys.time())))
url <- 'http://deftgeneralstorage.blob.core.windows.net/holograph/irisTest_test.hgd'
key <- '<>'
code <- paste(ms, url, key, sep='|')
code
qrcode_gen(code)
