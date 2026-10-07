import sys
import xml.etree.ElementTree as ET
for path in sys.argv[1:]:
    root=ET.parse(path).getroot()
    print(path, {k:root.get(k) for k in ('total','passed','failed','skipped')})
    for test in root.iter('test-case'):
        if test.get('result')=='Failed':
            print(test.get('name'), test.findtext('failure/message'), test.findtext('failure/stack-trace'))
