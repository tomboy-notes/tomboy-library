RELEASEVER=0.1.0
ZIPDIR=tomboy-$(RELEASEVER)
CONFIGURATION=Release
BINDIR=$(shell pwd)/Tomboy-library/bin/$(CONFIGURATION)
RELEASEDIR=$(shell pwd)/release

NUNIT_RUNNER="nuget-packages/NUnit.Runners.2.6.3/tools/nunit-console.exe"
MONO=$(shell which mono)
XBUILD=$(shell which xbuild)

XBUILD_ARGS='/p:Configuration=$(CONFIGURATION)'
MKBUNDLE=$(shell which mkbundle)

UNPACKED_LIB=$(BINDIR)/Tomboy-library.dll
PACKED_LIB=Tomboy-library.dll

# Note this is the min version for building from source; running might work
# on older mono versions
MIN_MONO_VERSION=3.0.0

pack: build
	@echo "Packing all assembly deps into the final .dll"
	$(MONO) ./tools/ILRepack.exe /out:$(RELEASEDIR)/$(PACKED_LIB) $(BINDIR)/ServiceStack.*.dll 
	@echo ""
	@echo "**********"
	@echo "Success! Find your executable in $(RELEASEDIR)/$(PACKED_LIB)"
	@echo "**********"
	@echo ""

deps:
	# if the next steps fails telling about security authentication, make sure
	# you have imported trusted ssl CA certs with this command and re-run:
	#
	# mozroots --import --sync
	#

	@mono tools/NuGet.exe install -o nuget-packages packages.config
	@echo "Successfully fetched dependencies."

build: deps

	$(XBUILD) $(XBUILD_ARGS) Tomboy-library.sln

test: deps build
	@mono $(NUNIT_RUNNER) ./Tomboy-library/Tomboy-library-tests/bin/$(CONFIGURATION)/Tomboy-library-tests.dll

release: clean pack
	cp -R $(RELEASEDIR) $(ZIPDIR)
	zip -r $(ZIPDIR).zip $(ZIPDIR)
	
clean:
	rm -rf Tomboy-library/obj/*
	rm -rf $(ZIPDIR)
	rm -rf $(ZIPDIR).zip
	rm -rf $(BINDIR)/*
	rm -rf $(RELEASEDIR)/*.dll
	rm -rf $(RELEASEDIR)/*.pdb
