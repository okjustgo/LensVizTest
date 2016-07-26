library(rClr)
library(dplyr)
library(qrcode)
library(ggplot2)

storageAccount <- 'deftgeneralstorage'
accountKey <- 'figuremeout'
container <- 'holograph'

getAuthQrCode <- function(storageAccountName, storageAccountKey) {
    code <- paste('auth', storageAccountName, storageAccountKey, sep='|')
    qrcode_gen(code, softLimitFlag=T)
}

getDatasetQrCode <- function(containerName, blobName) {
    ms <- as.character(as.integer(as.POSIXct(Sys.time())))
    code <- paste('graph', ms, containerName, blobName, sep='|')
    qrcode_gen(code, softLimitFlag=T)
}

# Set up data helper binaries and get authorization QR code
# Until this is a package, set this to the location of the RHoloGraphTransfer binary
rhgHelperPath <- "D:/LensVizTest/DataApp/RHoloGraphTransfer/bin/Debug/RHoloGraphTransfer.dll"
clrLoadAssembly(rhgHelperPath)
connStr <- paste('DefaultEndpointsProtocol=http;AccountName=', storageAccount, ';AccountKey=', accountKey, sep='')
getAuthQrCode(storageAccount, accountKey)

# Expects order of columns in dataFrame to be x;y;z;series
sendHgd <- function(dataFrame, plotTitle, geom, containerName, blobName) {
    tempPath <- tempfile('_deleteMe_rhgd_', getwd(), '.tsv')
    tempPath <- gsub('\\', '/', tempPath, fixed=TRUE)
    print(tempPath)
    write.table(dd, file=tempPath, row.names=F, sep='\t')
    
    obj <- clrNew('RHoloGraphTransfer.HoloGraphTransfer', connStr, containerName)
    
    clrCall(obj, 'UploadCsvAsHgd', tempPath, blobName, plotTitle, geom, "x;y;z;series")
    
    getDatasetQrCode(containerName, blobName)
}


# Iris scatterplot
dd <- iris %>% select(one_of('Sepal.Width', 'Sepal.Length', 'Petal.Length', 'Species'))
sendHgd(dd, 'Comparison of Iris Species', 'scatter', 'holograph', 'iris.hgd')

# Diamonds barplot
dd <- aggregate(price ~ cut + clarity, data=diamonds, FUN=median)
dd$cut <- sapply(dd$cut, function(x) { which(sort(unique(dd$cut)) == x) - 1 })
dd$clarity <- sapply(dd$clarity, function(x) { which(sort(unique(dd$clarity)) == x) - 1 })
dd$series <- 'diamonds'
sendHgd(dd, 'Price of Diamonds By Quality', 'bar', 'holograph', 'diamonds.hgd')

# Volcano surfaceplot
library(reshape2)
dd <- melt(volcano)
names(dd) <- c('Latitude', 'Longitude', 'Height')
dd$series <- 'volcano'
sendHgd(dd, 'Auckland\'s Maunga Whau Volcano', 'surface', 'holograph', 'volcano.hgd')

