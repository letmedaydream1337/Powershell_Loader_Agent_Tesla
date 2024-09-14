param(
    [string]$imagePath,      # The path to the image containing the embedded assembly
    [string]$className,      # The fully qualified class name from the assembly
    [string]$method,         # The method name to invoke
    [string]$argument        # Additional argument to pass to the method
)

# Read the image file as bytes and then convert to a string using UTF-8 encoding
$imageData = [System.IO.File]::ReadAllBytes($imagePath)
$imageDataString = [System.Text.Encoding]::UTF8.GetString($imageData)

# Search for the base64 encoded part
$startTag = "Redemption"
$endTag = "EndRedemption"

$startIndex = $imageDataString.IndexOf($startTag) + $startTag.Length
$endIndex = $imageDataString.IndexOf($endTag, $startIndex)
if ($startIndex -gt -1) {
    
    if ($endIndex -gt -1) {
        $base64Length = $endIndex - $startIndex
        $base64String = $imageDataString.Substring($startIndex, $base64Length)

        # Decode the base64 string to get the binary module data
        $moduleBytes = [System.Convert]::FromBase64String($base64String)

        # Load the assembly from the binary data
        $assembly = [System.Reflection.Assembly]::Load($moduleBytes)
        $type = $assembly.GetType($className)

        if ($type -ne $null) {
            # Call the static method
            try {
                # Get the type from the loaded assembly
                
                Write-Host " imagePath : " $imagePath
                Write-Host " className : " $className
                Write-Host " method : " $method
                Write-Host " argument : " $argument

                if ($method -ne $null) {
                    
                    $paramValues = [object[]]("$argument")
                    $result = $type.GetMethod($method, [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::Public).Invoke($null, $paramValues)
                }
                else {
                    Write-Error "Method '$method' not found in the class '$className'."
                }
            }
            catch {
                Write-Error "Error invoking the method: $_"
            }
        }
        else {
            Write-Error "Type '$className' not found in the assembly."
        }
    }
    else {
        Write-Error "End tag not found in the data."
    }
}
else {
    Write-Error "Start tag not found in the data."
}
