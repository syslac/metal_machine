#!/bin/zsh

# Original html to link-only
#cat setlist_list.html | grep "a href" | awk -F "\"" '{ print $2 }' | perl -pe 's/^../https:\/\/setlist.fm/' > setlist_list_link_only.txt

# Download saved links each into their separate file
#cat setlist_list_link_only.txt | while read i; do cd single_concerts; sleep 30; wget $i; cd ..; done;

echo "" > setlist_list_parsed.csv;
for file in single_concerts/*.html; do
  # get artist from file
  artist=`cat $file | grep qc:artist | awk -F "\"" '{ print $4 }'`;
  # get location from file
  location=`cat $file | grep qc:venue | awk -F "\"" '{ print $4 }'`;
  # get short location from file
  short_location=`cat $file | grep qc:venue | awk -F "\"" '{ print $4 }' | perl -pe 's/^.*?,\s*//'`;
  year=`cat $file | grep 'class="year"' | awk -F ">" '{ print $2 }' | awk -F "<" '{ print $1 }' | head -n 1`;

  echo "$artist;$location;$short_location;$year" >> setlist_list_parsed.csv;
done;
perl -pi -e "s/\\&\\#039;/\\'/g" setlist_list_parsed.csv;

