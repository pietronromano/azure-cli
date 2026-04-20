#!/bin/bash

# Script to search for APIs and operations using sql-data-source resolver in a specific APIM instance
# Usage: ./search-apim-sql-resolver.sh <apim-name> <resource-group> [subscription]

# Check arguments
if [ $# -lt 2 ]; then
    echo "Usage: $0 <apim-name> <resource-group> [subscription]"
    echo ""
    echo "Example:"
    echo "  $0 my-apim-instance my-resource-group"
    echo "  $0 my-apim-instance my-resource-group 12345678-1234-1234-1234-123456789abc"
    exit 1
fi

APIM_NAME=$1
RESOURCE_GROUP=$2
SUBSCRIPTION=${3:-""}

echo "========================================="
echo "Searching for sql-data-source resolver"
echo "========================================="
echo "APIM Instance: $APIM_NAME"
echo "Resource Group: $RESOURCE_GROUP"

# Set subscription if provided
if [ -n "$SUBSCRIPTION" ]; then
    echo "Subscription: $SUBSCRIPTION"
    echo ""
    echo "Setting subscription context..."
    set_result=$(az account set --subscription "$SUBSCRIPTION" 2>&1)
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to set subscription context"
        echo "Details: $set_result"
        echo ""
        echo "Checking if subscription exists and is accessible..."
        sub_info=$(az account show --subscription "$SUBSCRIPTION" --query "{id:id, name:name, state:state}" -o json 2>&1)
        if echo "$sub_info" | grep -qi "error\|not found"; then
            echo "Subscription not found or not accessible"
        else
            echo "Subscription info:"
            echo "$sub_info" | jq '.'
        fi
        exit 1
    fi
    echo "✓ Subscription context set successfully"
else
    current_sub=$(az account show --query id -o tsv 2>/dev/null)
    echo "Subscription: $current_sub (current)"
fi

echo ""
echo "========================================="

# Results file
results_file="sql-resolver-results-${APIM_NAME}.json"
echo "[]" > "$results_file"

# Get all APIs in this APIM instance
echo ""
echo "Step 1: Getting all APIs in $APIM_NAME..."
api_list=$(az apim api list --resource-group "$RESOURCE_GROUP" --service-name "$APIM_NAME" --query "[].{name:name, displayName:displayName, path:path}" -o json 2>&1)

# Check if command failed
if echo "$api_list" | grep -qi "error\|failed\|exception"; then
    echo "ERROR: Failed to query APIs"
    echo "$api_list"
    exit 1
fi

if [ "$api_list" == "[]" ] || [ "$api_list" == "" ] || [ "$api_list" == "null" ]; then
    echo "No APIs found in this APIM instance"
    exit 0
fi

# Count APIs
api_count=$(echo "$api_list" | jq '. | length')
echo "Found $api_count API(s)"
echo ""

echo "========================================="
echo "Step 2: Checking each API for sql-data-source resolver"
echo "========================================="
echo ""

found_count=0

# Iterate through each API
echo "$api_list" | jq -c '.[]' | while read api_obj; do
    api_id=$(echo "$api_obj" | jq -r '.name')
    api_display_name=$(echo "$api_obj" | jq -r '.displayName')
    api_path=$(echo "$api_obj" | jq -r '.path')
    
    echo "Checking API: $api_display_name"
    echo "  ID: $api_id"
    echo "  Path: $api_path"
    
    # Get API details including policies
    api_details=$(az apim api show --resource-group "$RESOURCE_GROUP" --service-name "$APIM_NAME" --api-id "$api_id" -o json 2>/dev/null)
    
    # Check API-level policy
    api_policy=$(echo "$api_details" | jq -r '.properties.policies // empty' 2>/dev/null)
    
    api_has_sql_resolver=false
    if echo "$api_policy" | grep -q "sql-data-source"; then
        echo "  ✓ API-level policy uses sql-data-source resolver"
        api_has_sql_resolver=true
        found_count=$((found_count + 1))
        
        # Store API-level result
        result=$(jq -n \
            --arg apim "$APIM_NAME" \
            --arg rg "$RESOURCE_GROUP" \
            --arg api "$api_id" \
            --arg displayName "$api_display_name" \
            --arg path "$api_path" \
            --arg level "API" \
            --arg operation "" \
            '{apim: $apim, resourceGroup: $rg, api: $api, apiDisplayName: $displayName, apiPath: $path, level: $level, operation: $operation}')
        
        jq ". += [$result]" "$results_file" > tmp.json && mv tmp.json "$results_file"
    fi
    
    # Get all operations for this API
    operations=$(az apim api operation list --resource-group "$RESOURCE_GROUP" --service-name "$APIM_NAME" --api-id "$api_id" --query "[].{name:name, displayName:displayName, method:method, urlTemplate:urlTemplate}" -o json 2>/dev/null)
    
    if [ "$operations" != "[]" ] && [ "$operations" != "" ] && [ "$operations" != "null" ]; then
        operation_count=$(echo "$operations" | jq '. | length')
        echo "  Checking $operation_count operation(s)..."
        
        # Check each operation
        echo "$operations" | jq -c '.[]' | while read op_obj; do
            op_id=$(echo "$op_obj" | jq -r '.name')
            op_display_name=$(echo "$op_obj" | jq -r '.displayName')
            op_method=$(echo "$op_obj" | jq -r '.method')
            op_url=$(echo "$op_obj" | jq -r '.urlTemplate')
            
            # Get operation policy
            op_policy=$(az apim api operation show --resource-group "$RESOURCE_GROUP" --service-name "$APIM_NAME" --api-id "$api_id" --operation-id "$op_id" --query "properties.policies" -o json 2>/dev/null)
            
            if echo "$op_policy" | grep -q "sql-data-source"; then
                echo "    ✓ OPERATION: $op_display_name ($op_method $op_url) uses sql-data-source resolver"
                found_count=$((found_count + 1))
                
                # Store operation-level result
                result=$(jq -n \
                    --arg apim "$APIM_NAME" \
                    --arg rg "$RESOURCE_GROUP" \
                    --arg api "$api_id" \
                    --arg displayName "$api_display_name" \
                    --arg path "$api_path" \
                    --arg level "Operation" \
                    --arg operation "$op_id" \
                    --arg opDisplayName "$op_display_name" \
                    --arg opMethod "$op_method" \
                    --arg opUrl "$op_url" \
                    '{apim: $apim, resourceGroup: $rg, api: $api, apiDisplayName: $displayName, apiPath: $path, level: $level, operation: $operation, operationDisplayName: $opDisplayName, operationMethod: $opMethod, operationUrl: $opUrl}')
                
                jq ". += [$result]" "$results_file" > tmp.json && mv tmp.json "$results_file"
            fi
        done
    fi
    
    if [ "$api_has_sql_resolver" = false ]; then
        echo "  No sql-data-source resolver found in this API"
    fi
    echo ""
done

echo ""
echo "========================================="
echo "Summary"
echo "========================================="
total_found=$(cat "$results_file" | jq '. | length')

if [ "$total_found" -gt 0 ]; then
    echo "✓ Found $total_found API(s)/Operation(s) with sql-data-source resolver"
    echo ""
    echo "Results saved to: $results_file"
    echo ""
    echo "Details:"
    cat "$results_file" | jq '.'
else
    echo "No APIs or operations with sql-data-source resolver found"
    rm -f "$results_file"
fi

echo "========================================="
